using MediatR;
using Microsoft.AspNetCore.Authorization;
using PentaBoard.Api.Features.Projects.DeleteProject;

namespace PentaBoard.Api.Features.Projects.DeleteProject;

public static class DeleteProjectEndpoint
{
    public static IEndpointRouteBuilder MapDeleteProjectEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/projects/{id:guid}", [Authorize] async (Guid id, IMediator mediator) =>
        {
            var ok = await mediator.Send(new DeleteProjectCommand(id));
            return ok ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProject")
        .WithTags("Projects");

        return app;
    }
}
