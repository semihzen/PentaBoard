using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using PentaBoard.Api.Domain.Entities;
using PentaBoard.Api.Features.WorkItems.Common;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.WorkItems.Create;

public class CreateWorkItemHandler : IRequestHandler<CreateWorkItemCommand, WorkItemDto>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public CreateWorkItemHandler(AppDbContext db, IHttpContextAccessor http)
    { _db = db; _http = http; }

    public async Task<WorkItemDto> Handle(CreateWorkItemCommand r, CancellationToken ct)
    {
        var principal = _http.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException();

        var uid =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
            principal.FindFirst("sub")?.Value;

        if (!Guid.TryParse(uid, out var reporterId))
            throw new InvalidOperationException("UserId claim missing/invalid");

        var board = await _db.Boards
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == r.BoardId && b.ProjectId == r.ProjectId, ct)
            ?? throw new InvalidOperationException("Board not found");

        var columnId = r.ColumnId
            ?? board.DefaultColumnId
            ?? await _db.BoardColumns
                .Where(c => c.BoardId == board.Id)
                .OrderBy(c => c.OrderKey)
                .Select(c => c.Id)
                .FirstAsync(ct);

        var nextOrder = (await _db.WorkItems
            .Where(w => w.BoardId == board.Id && w.BoardColumnId == columnId)
            .MaxAsync(w => (int?)w.OrderKey, ct) ?? 0) + 10;

        var wi = new WorkItem
        {
            ProjectId    = r.ProjectId,
            BoardId      = board.Id,
            BoardColumnId= columnId,
            Title        = r.Title.Trim(),
            Description  = string.IsNullOrWhiteSpace(r.Description) ? null : r.Description,
            Type         = string.IsNullOrWhiteSpace(r.Type) ? "Task" : r.Type,
            Priority     = r.Priority,
            ReporterId   = reporterId,
            AssigneeId   = r.AssigneeId,
            OrderKey     = nextOrder
        };

        _db.WorkItems.Add(wi);
        await _db.SaveChangesAsync(ct);

        return new WorkItemDto(
            wi.Id, wi.ProjectId, wi.BoardId, wi.BoardColumnId,
            wi.Title, wi.Description, wi.Type, wi.Priority, wi.OrderKey,
            wi.AssigneeId   // ✅ eksik argüman tamam
        );
    }
}
