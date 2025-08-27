using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Domain.Entities;
using PentaBoard.Api.Features.Boards.Common;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Boards.AddBoardColumn;

public sealed class AddBoardColumnHandler(
    AppDbContext db
) : IRequestHandler<AddBoardColumnCommand, BoardColumnDto>
{
    public async Task<BoardColumnDto> Handle(AddBoardColumnCommand req, CancellationToken ct)
    {
        // 1) Board'ı al / yoksa oluştur (ensure)
        var board = await db.Boards
            .FirstOrDefaultAsync(b => b.ProjectId == req.ProjectId, ct);

        if (board is null)
        {
            board = new Board { ProjectId = req.ProjectId, Name = "Default" };
            db.Boards.Add(board);
            await db.SaveChangesAsync(ct);
        }

        // 2) Mevcut kolonları çek (sıralı)
        var cols = await db.BoardColumns
            .Where(c => c.BoardId == board.Id)
            .OrderBy(c => c.OrderKey)
            .ToListAsync(ct);

        // 3) İsim validasyonu ve uniqueness (board içinde)
        var name = (req.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Column name is required.");

        var exists = cols.Any(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        if (exists)
            throw new InvalidOperationException("A column with the same name already exists on this board.");

        // 4) Ekleme konumunu belirle
        //    - InsertAfterColumnId verilmişse o kolondan sonra eklemeye çalış
        //    - Gap varsa 'ara'ya OrderKey verip direkt ekle
        //    - Gap yoksa: iki aşamalı normalize ile ekle (transaction)
        if (req.InsertAfterColumnId is Guid afterId)
        {
            var ordered = cols; // zaten OrderBy ile sıralı
            var idx = ordered.FindIndex(c => c.Id == afterId);

            if (idx >= 0)
            {
                var baseKey = ordered[idx].OrderKey;
                var nextKey = (idx + 1 < ordered.Count) ? ordered[idx + 1].OrderKey : baseKey + 10;

                // GAP VARSA: ortasına koyup bitir
                if (nextKey - baseKey > 1)
                {
                    var midKey = baseKey + (nextKey - baseKey) / 2;

                    var entityQuick = new BoardColumn
                    {
                        BoardId    = board.Id,
                        Name       = name,
                        Color      = string.IsNullOrWhiteSpace(req.Color) ? null : req.Color,
                        WipLimit   = req.WipLimit,
                        IsDoneLike = req.IsDoneLike ?? false,
                        OrderKey   = midKey
                    };

                    db.BoardColumns.Add(entityQuick);
                    await db.SaveChangesAsync(ct);

                    return new BoardColumnDto(
                        entityQuick.Id,
                        entityQuick.Name,
                        entityQuick.OrderKey,
                        entityQuick.Color,
                        entityQuick.WipLimit,
                        entityQuick.IsDefault,
                        entityQuick.IsDoneLike
                    );
                }

                // GAP YOKSA: iki aşamalı normalize
                return await InsertWithTwoPhaseNormalize(board.Id, name, req.Color, req.WipLimit, req.IsDoneLike, ct);
            }

            // afterId bulunamadıysa → sona ekle
            return await InsertAtEnd(board.Id, name, req.Color, req.WipLimit, req.IsDoneLike, ct);
        }
        else
        {
            // InsertAfter verilmemiş → sona ekle
            return await InsertAtEnd(board.Id, name, req.Color, req.WipLimit, req.IsDoneLike, ct);
        }
    }

    /// <summary>
    /// Sona eklerken gap olduğundan emin (max+10). Basit yol; normalize gerekmez.
    /// </summary>
    private async Task<BoardColumnDto> InsertAtEnd(
        Guid boardId,
        string name,
        string? color,
        int? wipLimit,
        bool? isDoneLike,
        CancellationToken ct)
    {
        var maxKey = await db.BoardColumns
            .Where(c => c.BoardId == boardId)
            .MaxAsync(c => (int?)c.OrderKey, ct) ?? 0;

        var entity = new BoardColumn
        {
            BoardId    = boardId,
            Name       = name,
            Color      = string.IsNullOrWhiteSpace(color) ? null : color,
            WipLimit   = wipLimit,
            IsDoneLike = isDoneLike ?? false,
            OrderKey   = maxKey + 10
        };

        db.BoardColumns.Add(entity);
        await db.SaveChangesAsync(ct);

        return new BoardColumnDto(
            entity.Id,
            entity.Name,
            entity.OrderKey,
            entity.Color,
            entity.WipLimit,
            entity.IsDefault,
            entity.IsDoneLike
        );
    }

    /// <summary>
    /// Gap yoksa EF'in "circular dependency" hatasına düşmemek için:
    /// 1) Tüm kolonlara geçici benzersiz OrderKey ver (max+100000 + i)
    /// 2) Yeni kolonu geçici dizinin sonuna ekle
    /// 3) Tüm kolonları 10,20,30... olacak şekilde normalize et
    /// </summary>
    private async Task<BoardColumnDto> InsertWithTwoPhaseNormalize(
        Guid boardId,
        string name,
        string? color,
        int? wipLimit,
        bool? isDoneLike,
        CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Board içindeki mevcut en büyük key'i al, üstüne 100000 ekle
        var tempBase = await db.BoardColumns
            .Where(x => x.BoardId == boardId)
            .MaxAsync(x => (int?)x.OrderKey, ct) ?? 0;
        tempBase += 100000;

        // Mevcut kolonları temp anahtarlara taşı
        var existing = await db.BoardColumns
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.OrderKey)
            .ToListAsync(ct);

        for (int i = 0; i < existing.Count; i++)
            existing[i].OrderKey = tempBase + i;

        await db.SaveChangesAsync(ct);

        // Yeni kolonu geçici olarak ekle (sonraki index)
        var entity = new BoardColumn
        {
            BoardId    = boardId,
            Name       = name,
            Color      = string.IsNullOrWhiteSpace(color) ? null : color,
            WipLimit   = wipLimit,
            IsDoneLike = isDoneLike ?? false,
            OrderKey   = tempBase + existing.Count
        };
        db.BoardColumns.Add(entity);
        await db.SaveChangesAsync(ct);

        // Nihai normalize: 10,20,30...
        var all = await db.BoardColumns
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.OrderKey)
            .ToListAsync(ct);

        int k = 0;
        int newOrderForInserted = 0;
        foreach (var c in all)
        {
            k += 10;
            c.OrderKey = k;
            if (c.Id == entity.Id) newOrderForInserted = k;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // DTO (normalize sonrası değeri döndür)
        return new BoardColumnDto(
            entity.Id,
            entity.Name,
            newOrderForInserted,
            entity.Color,
            entity.WipLimit,
            entity.IsDefault,
            entity.IsDoneLike
        );
    }
}
