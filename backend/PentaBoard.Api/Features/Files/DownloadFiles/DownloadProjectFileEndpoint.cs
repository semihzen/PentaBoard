using MediatR;

namespace PentaBoard.Api.Features.Files.DownloadFiles;

public static class DownloadProjectFileEndpoint
{
    public static IEndpointRouteBuilder MapDownloadProjectFileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/projects/{projectId:guid}/files/{id:guid}/download", async (Guid projectId, Guid id, IMediator mediator) =>
        {
            var res = await mediator.Send(new DownloadProjectFileQuery(projectId, id));
            return Results.File(File.OpenRead(res.AbsPath), res.ContentType, res.FileName);
        })
        .WithTags("ProjectFiles")
        .WithName("DownloadProjectFile");
        return app;
    }
}
