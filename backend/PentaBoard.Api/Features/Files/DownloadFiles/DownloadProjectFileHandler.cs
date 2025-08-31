using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Files.DownloadFiles;

public class DownloadProjectFileHandler : IRequestHandler<DownloadProjectFileQuery, DownloadFileResult>
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    public DownloadProjectFileHandler(AppDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public async Task<DownloadFileResult> Handle(DownloadProjectFileQuery r, CancellationToken ct)
    {
        var f = await _db.ProjectFiles.FirstOrDefaultAsync(x => x.Id == r.Id && x.ProjectId == r.ProjectId, ct);
        if (f is null) throw new FileNotFoundException();

        var webroot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var absPath = Path.Combine(webroot, f.StoragePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(absPath)) throw new FileNotFoundException();

        return new DownloadFileResult(absPath, f.ContentType, f.FileName);
    }
}
