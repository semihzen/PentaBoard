using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Domain.Entities;
using PentaBoard.Api.Features.Boards.Common;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Boards.GetBoard;

public sealed class GetBoardHandler(
    AppDbContext db,
    IHttpContextAccessor http
) : IRequestHandler<GetBoardQuery, BoardDto>
{
    public async Task<BoardDto> Handle(GetBoardQuery req, CancellationToken ct)
    {
        _ = http.HttpContext?.User ?? throw new UnauthorizedAccessException();

        // 1) Board'ı bul/yoksa oluştur
        var board = await db.Boards
            .FirstOrDefaultAsync(b => b.ProjectId == req.ProjectId, ct);

        if (board is null)
        {
            var callerIdStr = http.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)
                              ?? http.HttpContext!.User.FindFirstValue("sub")
                              ?? http.HttpContext!.User.FindFirstValue("id");
            Guid.TryParse(callerIdStr, out var callerId);

            board = new Board
            {
                ProjectId = req.ProjectId,
                Name = "Default",
                CreatedById = callerId
            };
            db.Boards.Add(board);
            await db.SaveChangesAsync(ct); // Id lazım
        }

        // 2) Kolon yoksa seed et
        var hasAnyColumn = await db.BoardColumns.AnyAsync(c => c.BoardId == board.Id, ct);
        if (!hasAnyColumn)
        {
            db.BoardColumns.AddRange(
                new BoardColumn { BoardId = board.Id, Name = "To Do", OrderKey = 10, IsDefault = true },
                new BoardColumn { BoardId = board.Id, Name = "Doing", OrderKey = 20 },
                new BoardColumn { BoardId = board.Id, Name = "Done",  OrderKey = 30, IsDoneLike = true }
            );
            await db.SaveChangesAsync(ct);
        }

        // 3) Kolonları doğrudan tablodan oku (Include'a bağlı kalma)
        var cols = await db.BoardColumns
            .Where(c => c.BoardId == board.Id)
            .OrderBy(c => c.OrderKey)
            .ToListAsync(ct);

        // 4) DTO
        return new BoardDto(
            board.Id,
            board.ProjectId,
            board.Name,
            cols.Select(c => new BoardColumnDto(
                    c.Id, c.Name, c.OrderKey, c.Color, c.WipLimit, c.IsDefault, c.IsDoneLike
                ))
                .ToList()
        );
    }
}
