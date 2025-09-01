using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.WorkItems.TaskState;

public sealed class GetTaskStateHandler : IRequestHandler<GetTaskStateQuery, TaskStateDto>
{
    private readonly AppDbContext _db;
    public GetTaskStateHandler(AppDbContext db) => _db = db;

    public async Task<TaskStateDto> Handle(GetTaskStateQuery q, CancellationToken ct)
    {
        var sinceUtc = DateTime.UtcNow.AddDays(-Math.Max(1, q.Days));

        // Board + kolonları (OrderKey'leri ile)
        var board = await _db.Boards
            .Include(b => b.Columns)
            .FirstOrDefaultAsync(b => b.Id == q.BoardId && b.ProjectId == q.ProjectId, ct);

        if (board is null)
            return new TaskStateDto(0, 0, 0, Array.Empty<RecentItemDto>());

        // DONE kolonu: öncelik Board.DoneColumnId, yoksa en sağdaki (OrderKey max)
        var doneColumnId =
            board.DoneColumnId
            ?? board.Columns
                   .OrderBy(c => c.OrderKey)    // 10,20,30... en büyük = sağdaki
                   .Select(c => c.Id)
                   .LastOrDefault();             // bulunamazsa Guid.Empty döner

        // --- Sayımlar ---
        var createdCount = await _db.WorkItems.CountAsync(
            w => w.ProjectId == q.ProjectId
              && w.BoardId   == q.BoardId
              && w.CreatedAt >= sinceUtc, ct); // CreatedAt alanı kullanılır  :contentReference[oaicite:3]{index=3}

        var completedCount = doneColumnId == Guid.Empty
            ? 0
            : await _db.WorkItems.CountAsync(
                w => w.ProjectId == q.ProjectId
                  && w.BoardId   == q.BoardId
                  && w.BoardColumnId == doneColumnId, ct);

        var inProgressCount = doneColumnId == Guid.Empty
            ? await _db.WorkItems.CountAsync(w => w.ProjectId == q.ProjectId && w.BoardId == q.BoardId, ct)
            : await _db.WorkItems.CountAsync(
                w => w.ProjectId == q.ProjectId
                  && w.BoardId   == q.BoardId
                  && w.BoardColumnId != doneColumnId, ct);

        // (İstersen "recent"i kapatabilirsin; frontend göstermiyor ama DTO'da yer bırakıyorum)
        var recent = await _db.WorkItems
            .Where(w => w.ProjectId == q.ProjectId
                     && w.BoardId   == q.BoardId
                     && w.CreatedAt >= sinceUtc)
            .OrderByDescending(w => w.CreatedAt)
            .Take(10)
            .Select(w => new RecentItemDto(
                w.Id, w.Title, w.CreatedAt,
                (doneColumnId != Guid.Empty && w.BoardColumnId == doneColumnId) ? "completed" : "işlemde"
            ))
            .ToListAsync(ct);

        return new TaskStateDto(createdCount, completedCount, inProgressCount, recent);
    }
}
