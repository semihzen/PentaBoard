using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Files.PreviewFiles;

public class PreviewProjectFileHandler : IRequestHandler<PreviewProjectFileQuery, PreviewFileResult>
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;

    public PreviewProjectFileHandler(AppDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<PreviewFileResult> Handle(PreviewProjectFileQuery r, CancellationToken ct)
    {
        var f = await _db.ProjectFiles
            .FirstOrDefaultAsync(x => x.Id == r.Id && x.ProjectId == r.ProjectId, ct);

        if (f is null)
            throw new FileNotFoundException("Dosya bulunamadı.");

        var webroot = _env.WebRootPath ?? Path.Combine(AppContext.BaseDirectory, "wwwroot");
        var absPath = Path.Combine(webroot, f.StoragePath.TrimStart('/')
            .Replace('/', Path.DirectorySeparatorChar));

        if (!File.Exists(absPath))
            throw new FileNotFoundException("Dosya yolu bulunamadı.");

        // filename vermeyeceğiz => tarayıcı inline açar
        return new PreviewFileResult(absPath, f.ContentType);
    }
}
