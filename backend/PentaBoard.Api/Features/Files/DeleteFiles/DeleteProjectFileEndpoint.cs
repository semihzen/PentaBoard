using MediatR;

namespace PentaBoard.Api.Features.Files.DeleteFiles;

public static class DeleteProjectFileEndpoint
{
    public static IEndpointRouteBuilder MapDeleteProjectFileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/projects/{projectId:guid}/files/{id:guid}", async (Guid projectId, Guid id, IMediator mediator) =>
        {
            var ok = await mediator.Send(new DeleteProjectFileCommand(projectId, id));
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .WithTags("ProjectFiles")
        .WithName("DeleteProjectFile");
        return app;
    }
}
