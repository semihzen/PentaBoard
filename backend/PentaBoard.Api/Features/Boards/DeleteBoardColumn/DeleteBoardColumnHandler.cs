using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Features.Boards.Common;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Boards.DeleteBoardColumn;

public sealed class DeleteBoardColumnHandler(AppDbContext db)
    : IRequestHandler<DeleteBoardColumnCommand, IReadOnlyList<BoardColumnDto>>
{
    public async Task<IReadOnlyList<BoardColumnDto>> Handle(DeleteBoardColumnCommand req, CancellationToken ct)
    {
        var board = await db.Boards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ProjectId == req.ProjectId, ct)
            ?? throw new KeyNotFoundException("Board not found.");

        var col = await db.BoardColumns
            .FirstOrDefaultAsync(c => c.Id == req.ColumnId && c.BoardId == board.Id, ct)
            ?? throw new KeyNotFoundException("Column not found.");

        db.BoardColumns.Remove(col);
        await db.SaveChangesAsync(ct);

        // kolonlar kaldıysa sırayı temizle (2-aşamalı: temp → normalize)
        var cols = await db.BoardColumns
            .Where(c => c.BoardId == board.Id)
            .OrderBy(c => c.OrderKey)
            .ToListAsync(ct);

        if (cols.Count > 0)
        {
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            var tempBase = (await db.BoardColumns
                                .Where(x => x.BoardId == board.Id)
                                .MaxAsync(x => (int?)x.OrderKey, ct)) ?? 0;
            tempBase += 100000;

            for (int i = 0; i < cols.Count; i++)
                cols[i].OrderKey = tempBase + i;

            await db.SaveChangesAsync(ct);

            var k = 0;
            for (int i = 0; i < cols.Count; i++)
            {
                k += 10;
                cols[i].OrderKey = k;
            }

            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }

        return (await db.BoardColumns
            .Where(c => c.BoardId == board.Id)
            .OrderBy(c => c.OrderKey)
            .Select(c => new BoardColumnDto(
                c.Id, c.Name, c.OrderKey, c.Color, c.WipLimit, c.IsDefault, c.IsDoneLike
            ))
            .ToListAsync(ct));
    }
}
