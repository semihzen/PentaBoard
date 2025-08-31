using MediatR;

namespace PentaBoard.Api.Features.Files.ListFiles;

public static class ListProjectFilesEndpoint
{
    public static IEndpointRouteBuilder MapListProjectFilesEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/projects/{projectId:guid}/files", async (Guid projectId, IMediator mediator)
            => Results.Ok(await mediator.Send(new ListProjectFilesQuery(projectId))))
           .WithTags("ProjectFiles")
           .WithName("ListProjectFiles");
        return app;
    }
}
