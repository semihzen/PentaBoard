using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Domain;               // ProjectMember
using PentaBoard.Api.Infrastructure;       // AppDbContext

namespace PentaBoard.Api.Features.Members.AddProjectMember;

public sealed class AddProjectMemberHandler(
    AppDbContext db,
    IHttpContextAccessor http
) : IRequestHandler<AddProjectMemberCommand, Unit>
{
    public async Task<Unit> Handle(AddProjectMemberCommand req, CancellationToken ct)
    {
        var user = http.HttpContext?.User ?? throw new UnauthorizedAccessException();
        var role = (user.FindFirst(ClaimTypes.Role)?.Value ?? "").Trim().ToLowerInvariant();
        var callerIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(callerIdStr, out var callerId))
            throw new UnauthorizedAccessException();

        var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == req.ProjectId, ct)
            ?? throw new KeyNotFoundException("Project not found");

        var isAdmin = role == "admin";
        var isSystemAdminOwner = role == "system admin" && project.ProjectAdminId == callerId;
        if (!isAdmin && !isSystemAdminOwner)
            throw new UnauthorizedAccessException();

        var userExists = await db.Users.AnyAsync(u => u.Id == req.UserId, ct);
        if (!userExists) throw new InvalidOperationException("User not found");

        var already = await db.ProjectMembers
            .AnyAsync(m => m.ProjectId == req.ProjectId && m.UserId == req.UserId, ct);
        if (already) return Unit.Value;

        db.ProjectMembers.Add(new ProjectMember
        {
            ProjectId = req.ProjectId,
            UserId    = req.UserId,
            SubRole   = string.IsNullOrWhiteSpace(req.SubRole) ? "member" : req.SubRole.Trim(),
            AddedById = callerId
        });

        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
