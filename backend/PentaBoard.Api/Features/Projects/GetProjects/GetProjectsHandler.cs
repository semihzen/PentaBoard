using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Projects.GetProjects;

public class GetProjectsHandler : IRequestHandler<GetProjectsQuery, List<GetProjectsResult>>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public GetProjectsHandler(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task<List<GetProjectsResult>> Handle(GetProjectsQuery request, CancellationToken ct)
    {
        var user = _http.HttpContext!.User;

        var userIdStr = user.FindFirst("uid")?.Value
                        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdStr))
            throw new UnauthorizedAccessException("User id claim missing.");

        var currentUserId = Guid.Parse(userIdStr);

        var currentUser = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == currentUserId, ct)
            ?? throw new UnauthorizedAccessException("User not found.");

        var role = (currentUser.Role ?? string.Empty).Trim().ToLowerInvariant();
        var isAdmin = role == "admin";
        var isSystemAdmin = role == "system admin";

        IQueryable<Domain.Project> query = _db.Projects.AsNoTracking();

        if (isAdmin)
        {
            // Admin: tüm projeler
        }
        else if (isSystemAdmin)
        {
            // System Admin: kendisine atanmış projeler
            query = query.Where(p => p.ProjectAdminId == currentUserId);
        }
        else
        {
            // User: şimdilik proje göstermiyoruz (ileride membership ile açılacak)
            return new List<GetProjectsResult>();
        }

        // 1) EF'nin çevirebildiği yalın alanları çek
        var raw = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Key,
                p.Description,
                p.Color,
                p.StartDate,
                p.EndDate,
                p.Tags,
                p.ProjectAdminId,
                p.CreatedById,
                p.CreatedAt
            })
            .ToListAsync(ct);

        // 2) Materialize olduktan sonra Tags'i parçala ve DTO'ya map et
        var results = raw.Select(p => new GetProjectsResult(
            p.Id,
            p.Name,
            p.Key,
            p.Description,
            p.Color,
            p.StartDate,
            p.EndDate,
            string.IsNullOrWhiteSpace(p.Tags)
                ? null
                : p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => s.Length > 0)
                        .ToList(),
            p.ProjectAdminId,
            p.CreatedById,
            p.CreatedAt
        )).ToList();

        return results;
    }
}
