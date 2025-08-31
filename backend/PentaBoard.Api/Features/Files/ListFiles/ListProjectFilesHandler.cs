using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Files.ListFiles;

public class ListProjectFilesHandler : IRequestHandler<ListProjectFilesQuery, IReadOnlyList<ProjectFileDto>>
{
    private readonly AppDbContext _db;
    public ListProjectFilesHandler(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<ProjectFileDto>> Handle(ListProjectFilesQuery req, CancellationToken ct)
    {
        return await _db.ProjectFiles
            .Where(f => f.ProjectId == req.ProjectId)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new ProjectFileDto(f.Id, f.FileName, f.ContentType, f.SizeBytes, f.CreatedAt))
            .ToListAsync(ct);
    }
}
