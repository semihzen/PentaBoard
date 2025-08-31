using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Files.DeleteFiles;

public class DeleteProjectFileHandler : IRequestHandler<DeleteProjectFileCommand, bool>
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    public DeleteProjectFileHandler(AppDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public async Task<bool> Handle(DeleteProjectFileCommand r, CancellationToken ct)
    {
        var f = await _db.ProjectFiles.FirstOrDefaultAsync(x => x.Id == r.Id && x.ProjectId == r.ProjectId, ct);
        if (f is null) return false;

        _db.ProjectFiles.Remove(f);
        await _db.SaveChangesAsync(ct);

        var webroot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var absPath = Path.Combine(webroot, f.StoragePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(absPath)) File.Delete(absPath);

        return true;
    }
}
