// Features/WorkItems/Get/GetWorkItemByIdHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Features.WorkItems.Common;
using PentaBoard.Api.Infrastructure; 

namespace PentaBoard.Api.Features.WorkItems.Get;

public sealed class GetWorkItemByIdHandler 
    : IRequestHandler<GetWorkItemByIdQuery, WorkItemDto?>
{
    private readonly AppDbContext _db;
    public GetWorkItemByIdHandler(AppDbContext db) => _db = db;

    public async Task<WorkItemDto?> Handle(GetWorkItemByIdQuery q, CancellationToken ct)
    {
        return await _db.WorkItems
            .Where(w => w.Id == q.WorkItemId 
                        && w.ProjectId == q.ProjectId 
                        && w.BoardId == q.BoardId)
            .Select(w => new WorkItemDto(
                w.Id,
                w.ProjectId,
                w.BoardId,
                w.BoardColumnId,
                w.Title,
                w.Description,
                w.Type,
                w.Priority,
                w.OrderKey,
                w.AssigneeId
            ))
            .FirstOrDefaultAsync(ct);
    }
}
