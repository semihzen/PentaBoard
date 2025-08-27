using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Domain.Entities;
using PentaBoard.Api.Features.Boards.Common;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Boards.MoveBoardColumn;

public sealed class MoveBoardColumnHandler(AppDbContext db)
    : IRequestHandler<MoveBoardColumnCommand, IReadOnlyList<BoardColumnDto>>
{
    public async Task<IReadOnlyList<BoardColumnDto>> Handle(MoveBoardColumnCommand req, CancellationToken ct)
    {
        // Board
        var board = await db.Boards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ProjectId == req.ProjectId, ct)
            ?? throw new KeyNotFoundException("Board not found.");

        // Kolonları sırayla çek
        var cols = await db.BoardColumns
            .Where(c => c.BoardId == board.Id)
            .OrderBy(c => c.OrderKey)
            .ToListAsync(ct);

        var moving = cols.FirstOrDefault(c => c.Id == req.ColumnId)
            ?? throw new KeyNotFoundException("Column not found.");

        // Listeden çıkar, hedefe ekle
        cols.Remove(moving);
        var target = Math.Clamp(req.TargetIndex, 0, cols.Count);
        cols.Insert(target, moving);

        // === İKİ AŞAMALI NORMALİZASYON ===
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // 1) Geçici benzersiz anahtarlar (çakışmayı önle)
        // Board içindeki en büyük key + 100000 baz al
        var tempBase = (await db.BoardColumns
                          .Where(x => x.BoardId == board.Id)
                          .MaxAsync(x => (int?)x.OrderKey, ct)) ?? 0;
        tempBase += 100000;

        for (int i = 0; i < cols.Count; i++)
            cols[i].OrderKey = tempBase + i;

        await db.SaveChangesAsync(ct);

        // 2) Nihai 10,20,30… anahtarları ver
        var k = 0;
        for (int i = 0; i < cols.Count; i++)
        {
            k += 10;
            cols[i].OrderKey = k;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // DTO
        return cols
            .OrderBy(c => c.OrderKey)
            .Select(c => new BoardColumnDto(
                c.Id, c.Name, c.OrderKey, c.Color, c.WipLimit, c.IsDefault, c.IsDoneLike
            ))
            .ToList();
    }
}
