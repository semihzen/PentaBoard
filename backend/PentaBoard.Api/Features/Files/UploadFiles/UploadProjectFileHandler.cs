using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Files.UploadFiles;

public class UploadProjectFileHandler : IRequestHandler<UploadProjectFileCommand, ProjectFileCreatedDto>
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    public UploadProjectFileHandler(AppDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public async Task<ProjectFileCreatedDto> Handle(UploadProjectFileCommand r, CancellationToken ct)
    {
        // 🔒 FK guard: UploadedById gerçekten Users'ta var mı?
        var userExists = await _db.Users.AnyAsync(u => u.Id == r.UploadedById, ct);
        if (!userExists)
            throw new InvalidOperationException("Kullanıcı bulunamadı veya yetkisiz.");

        if (r.File is null || r.File.Length == 0)
            throw new InvalidOperationException("Dosya bulunamadı.");

        var isPdfByMime = string.Equals(r.File.ContentType, "application/pdf", StringComparison.OrdinalIgnoreCase);
        var isPdfByExt  = Path.GetExtension(r.File.FileName).Equals(".pdf", StringComparison.OrdinalIgnoreCase);
        if (!isPdfByMime && !isPdfByExt)
            throw new InvalidOperationException("Sadece PDF yükleyebilirsiniz.");

        var webroot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var dir = Path.Combine(webroot, "uploads", "projects", r.ProjectId.ToString());
        Directory.CreateDirectory(dir);

        var newName = $"{Guid.NewGuid():N}.pdf";
        var absPath = Path.Combine(dir, newName);
        using (var s = File.Create(absPath))
            await r.File.CopyToAsync(s, ct);

        var entity = new Domain.Entities.ProjectFile
        {
            ProjectId    = r.ProjectId,       // -> Projects.Id
            UploadedById = r.UploadedById,    // -> Users.Id (FK guaranteed)
            FileName     = Path.GetFileName(r.File.FileName),
            ContentType  = "application/pdf",
            SizeBytes    = r.File.Length,
            StoragePath  = $"/uploads/projects/{r.ProjectId}/{newName}",
            CreatedAt    = DateTime.UtcNow
        };

        _db.ProjectFiles.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ProjectFileCreatedDto(entity.Id, entity.FileName, entity.ContentType, entity.SizeBytes, entity.CreatedAt);
    }
}
