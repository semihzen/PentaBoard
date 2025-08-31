using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace PentaBoard.Api.Features.Files.PreviewFiles;

public static class PreviewProjectFileEndpoint
{
    public static IEndpointRouteBuilder MapPreviewProjectFileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/projects/{projectId:guid}/files/{id:guid}/preview",
            [Authorize] async (Guid projectId, Guid id, IMediator mediator) =>
            {
                var res = await mediator.Send(new PreviewProjectFileQuery(projectId, id));
                // 🔸 filename vermiyoruz -> inline render
                return Results.File(File.OpenRead(res.AbsPath), res.ContentType);
            })
           .WithTags("ProjectFiles")
           .WithName("PreviewProjectFile");

        return app;
    }
}
