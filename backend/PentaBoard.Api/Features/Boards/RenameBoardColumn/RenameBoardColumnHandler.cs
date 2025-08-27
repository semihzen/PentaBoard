using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Features.Boards.Common;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Boards.RenameBoardColumn;

public sealed class RenameBoardColumnHandler(AppDbContext db)
    : IRequestHandler<RenameBoardColumnCommand, BoardColumnDto>
{
    public async Task<BoardColumnDto> Handle(RenameBoardColumnCommand req, CancellationToken ct)
    {
        var newName = (req.Name ?? "").Trim();
        if (string.IsNullOrWhiteSpace(newName))
            throw new InvalidOperationException("Name is required.");
        if (newName.Length > 100)
            throw new InvalidOperationException("Name is too long.");

        var board = await db.Boards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.ProjectId == req.ProjectId, ct)
            ?? throw new KeyNotFoundException("Board not found.");

        var col = await db.BoardColumns
            .FirstOrDefaultAsync(c => c.Id == req.ColumnId && c.BoardId == board.Id, ct)
            ?? throw new KeyNotFoundException("Column not found.");

        // aynı board içinde ad uniq olsun (case-insensitive)
        var exists = await db.BoardColumns.AnyAsync(c =>
            c.BoardId == board.Id &&
            c.Id != col.Id &&
            c.Name.ToLower() == newName.ToLower(), ct);

        if (exists)
            throw new InvalidOperationException("A column with the same name already exists on this board.");

        col.Name = newName;
        await db.SaveChangesAsync(ct);

        return new BoardColumnDto(
            col.Id, col.Name, col.OrderKey, col.Color, col.WipLimit, col.IsDefault, col.IsDoneLike
        );
    }
}
