using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Members.SetMemberRole;

public sealed class SetMemberRoleHandler(
    AppDbContext db,
    IHttpContextAccessor http
) : IRequestHandler<SetMemberRoleCommand, Unit>
{
    public async Task<Unit> Handle(SetMemberRoleCommand req, CancellationToken ct)
    {
        // ---- Kimlik
        var user = http.HttpContext?.User ?? throw new UnauthorizedAccessException();
        var role = (user.FindFirst(ClaimTypes.Role)?.Value ?? "").Trim().ToLowerInvariant();

        var callerIdStr =
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            user.FindFirstValue("sub") ??
            user.FindFirstValue("id");

        if (!Guid.TryParse(callerIdStr, out var callerId))
            throw new UnauthorizedAccessException();

        // ---- Proje
        var proj = await db.Projects
            .FirstOrDefaultAsync(p => p.Id == req.ProjectId, ct)
            ?? throw new KeyNotFoundException("Project not found");

        // Owner değiştirilemez
        if (req.TargetUserId == proj.ProjectAdminId)
            throw new InvalidOperationException("Owner role cannot be changed.");

        // ---- Yetki: admin || (system admin && proje sahibi)
        var isAdmin = role == "admin";
        var isOwnerSystemAdmin = role == "system admin" && proj.ProjectAdminId == callerId;
        if (!(isAdmin || isOwnerSystemAdmin))
            throw new UnauthorizedAccessException();

        // ---- Üyelik
        var membership = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == req.ProjectId && m.UserId == req.TargetUserId, ct);
        if (membership is null)
            throw new KeyNotFoundException("Membership not found.");

        // ---- SubRole: whitelist YOK — serbest metin
        var clean = (req.NewSubRole ?? "").Trim();
        if (string.IsNullOrEmpty(clean)) clean = "member";      // boşsa member
        if (clean.Length > 64) clean = clean[..64];            // aşırı uzunluğu kısalt (opsiyonel)

        // idempotent
        if (!string.Equals(membership.SubRole, clean, StringComparison.Ordinal))
        {
            membership.SubRole = clean;
            await db.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }
}
