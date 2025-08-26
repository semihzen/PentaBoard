using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure; // AppDbContext
// using PentaBoard.Api.Domain; // gerekirse

namespace PentaBoard.Api.Features.Projects.UpdateProject;

public class UpdateProjectHandler : IRequestHandler<UpdateProjectCommand, UpdateProjectResult>
{
    private readonly AppDbContext _db;
    public UpdateProjectHandler(AppDbContext db) => _db = db;

    public async Task<UpdateProjectResult> Handle(UpdateProjectCommand req, CancellationToken ct)
    {
        var p = await _db.Projects.FirstOrDefaultAsync(x => x.Id == req.Id, ct)
                ?? throw new KeyNotFoundException("Project not found.");

        // Sadece gönderilen alanları güncelle
        if (req.Description is not null)
            p.Description = req.Description;

        if (req.StartDate.HasValue)
            p.StartDate = req.StartDate;          // DateTime? → DateTime?

        if (req.EndDate.HasValue)
            p.EndDate = req.EndDate;              // DateTime? → DateTime?

        if (req.Tags is not null)
        {
            // List<string> → csv string
            var csv = string.Join(',', req.Tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim()));
            p.Tags = string.IsNullOrWhiteSpace(csv) ? null : csv;
        }

        await _db.SaveChangesAsync(ct);

        // Entity’de csv; DTO’da list
        var tagsOut = string.IsNullOrWhiteSpace(p.Tags)
            ? Array.Empty<string>()
            : p.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => s.Length > 0)
                    .ToArray();

        return new UpdateProjectResult(
            p.Id, p.Name, p.Key, p.Description, p.Color,
            p.StartDate, p.EndDate, tagsOut,
            p.ProjectAdminId, p.CreatedById, p.CreatedAt
        );
    }
}
