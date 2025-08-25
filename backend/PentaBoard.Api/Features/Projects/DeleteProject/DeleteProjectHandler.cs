using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Projects.DeleteProject;

public class DeleteProjectHandler : IRequestHandler<DeleteProjectCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public DeleteProjectHandler(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task<bool> Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        var user = _http.HttpContext!.User;

        // uid veya NameIdentifier claim'i
        var uid = user.FindFirst("uid")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(uid)) throw new UnauthorizedAccessException("User id claim missing.");
        var meId = Guid.Parse(uid);

        var meRole = (user.FindFirst(ClaimTypes.Role)?.Value ?? "").Trim();

        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.ProjectId, ct);
        if (project == null) return false;

        var isAdmin = meRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        var isSystemAdmin = meRole.Equals("System Admin", StringComparison.OrdinalIgnoreCase);

        if (!isAdmin)
        {
            // SA sadece kendi atanmış olduğu projeyi silebilir
            if (!(isSystemAdmin && project.ProjectAdminId == meId))
                throw new UnauthorizedAccessException("You are not allowed to delete this project.");
        }

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
