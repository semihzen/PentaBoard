using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Features.WorkItems.Common;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.WorkItems.Move;

public sealed class MoveWorkItemHandler : IRequestHandler<MoveWorkItemCommand, WorkItemDto>
{
    private readonly AppDbContext _db;
    public MoveWorkItemHandler(AppDbContext db) => _db = db;

    public async Task<WorkItemDto> Handle(MoveWorkItemCommand r, CancellationToken ct)
    {
        var wi = await _db.WorkItems
            .FirstOrDefaultAsync(x => x.Id == r.WorkItemId 
                                   && x.BoardId == r.BoardId
                                   && x.ProjectId == r.ProjectId, ct)
            ?? throw new InvalidOperationException("Work item not found.");

        var targetCol = await _db.BoardColumns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == r.TargetColumnId && c.BoardId == r.BoardId, ct)
            ?? throw new InvalidOperationException("Target column not found.");

        int nextOrder = (await _db.WorkItems
            .Where(w => w.BoardId == r.BoardId && w.BoardColumnId == r.TargetColumnId)
            .MaxAsync(w => (int?)w.OrderKey, ct) ?? 0) + 10;

        wi.BoardColumnId = r.TargetColumnId;
        wi.OrderKey      = nextOrder;
        wi.UpdatedAt     = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new WorkItemDto(
            wi.Id, wi.ProjectId, wi.BoardId, wi.BoardColumnId,
            wi.Title, wi.Description, wi.Type, wi.Priority, wi.OrderKey,
            wi.AssigneeId   // ✅ eksik argüman tamam
        );
    }
}
