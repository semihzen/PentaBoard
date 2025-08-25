using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Domain;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Projects.CreateProject;

public class CreateProjectHandler : IRequestHandler<CreateProjectCommand, CreateProjectResult>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public CreateProjectHandler(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task<CreateProjectResult> Handle(CreateProjectCommand request, CancellationToken ct)
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

        var role = (currentUser.Role ?? "").Trim();

        // slugify basit
        string key = request.Name.Trim()
            .ToLowerInvariant()
            .Replace('ş','s').Replace('ı','i').Replace('ç','c').Replace('ö','o').Replace('ü','u').Replace('ğ','g')
            .Replace(" ", "-")
            .Replace(".", "")
            .Replace(",", "");

        // Projenin sahibi olacak System Admin (ProjectAdmin)
        Guid projectAdminId;

        if (role.Equals("System Admin", StringComparison.OrdinalIgnoreCase))
        {
            // System Admin kendi projesini oluşturuyor → sahibi kendisi
            projectAdminId = currentUserId;
        }
        else if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            // Admin ise, hedef System Admin ID zorunlu
            if (request.ProjectAdminId is null)
                throw new InvalidOperationException("ProjectAdminId must be provided by Admin.");

            // hedef kullanıcının gerçekten System Admin olduğuna emin ol
            var target = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == request.ProjectAdminId.Value, ct)
                ?? throw new InvalidOperationException("Target system admin not found.");

            if (!target.Role.Equals("System Admin", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Target user must have 'System Admin' role.");

            projectAdminId = target.Id;
        }
        else
        {
            throw new UnauthorizedAccessException("Only Admin or System Admin can create a project.");
        }

        var project = new Project
        {
            Name = request.Name.Trim(),
            Key = key,
            Description = request.Description?.Trim(),
            Color = string.IsNullOrWhiteSpace(request.Color) ? "teal" : request.Color,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Tags = request.Tags is { Count: > 0 } ? string.Join(",", request.Tags) : null,

            ProjectAdminId = projectAdminId,     // sahip
            CreatedById = currentUserId          // oluşturan
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(ct);

        return new CreateProjectResult(project.Id, project.Name, project.Key);
    }
}
