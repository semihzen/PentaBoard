using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;     // AppDbContext

namespace PentaBoard.Api.Features.WorkItems.Delete;

public sealed class DeleteWorkItemHandler : IRequestHandler<DeleteWorkItemCommand, bool>
{
    private readonly AppDbContext _db;
    public DeleteWorkItemHandler(AppDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteWorkItemCommand r, CancellationToken ct)
    {
        // Work item gerçekten bu projedeki, bu board’a ait mi?
        var wi = await _db.WorkItems
            .Where(x => x.Id == r.WorkItemId &&
                        x.ProjectId == r.ProjectId &&
                        x.BoardId == r.BoardId)
            .FirstOrDefaultAsync(ct);

        if (wi is null) return false;

        _db.WorkItems.Remove(wi);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
