using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Members.RemoveProjectMember;

public sealed class RemoveProjectMemberHandler(
    AppDbContext db,
    IHttpContextAccessor http
) : IRequestHandler<RemoveProjectMemberCommand, Unit>
{
    public async Task<Unit> Handle(RemoveProjectMemberCommand req, CancellationToken ct)
    {
        // ——— Kimlik
        var user = http.HttpContext?.User ?? throw new UnauthorizedAccessException();
        var role = (user.FindFirst(ClaimTypes.Role)?.Value ?? "").Trim().ToLowerInvariant();

        var callerIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? user.FindFirstValue("sub")
                       ?? user.FindFirstValue("id");

        if (!Guid.TryParse(callerIdStr, out var callerId))
            throw new UnauthorizedAccessException();

        // ——— Proje
        var proj = await db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == req.ProjectId, ct)
            ?? throw new KeyNotFoundException("Project not found");

        // ——— Yetki: admin || (system admin && owner)
        var isAdmin = role == "admin";
        var isOwnerSystemAdmin = role == "system admin" && proj.ProjectAdminId == callerId;

        // ——— Owner asla silinemez
        if (req.TargetUserId == proj.ProjectAdminId)
            throw new InvalidOperationException("Owner cannot be removed.");

        // ——— “Kendi kendini çıkarma” serbest; aksi halde yukarıdaki yetkiler gerekli
        var removingSelf = req.TargetUserId == callerId;
        if (!removingSelf && !(isAdmin || isOwnerSystemAdmin))
            throw new UnauthorizedAccessException();

        // ——— Üyelik var mı?
        var membership = await db.ProjectMembers
            .FirstOrDefaultAsync(m => m.ProjectId == req.ProjectId && m.UserId == req.TargetUserId, ct);

        // Idempotent davran: yoksa sessizce OK dön
        if (membership is null) return Unit.Value;

        db.ProjectMembers.Remove(membership);
        await db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
