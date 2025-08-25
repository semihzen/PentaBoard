using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace PentaBoard.Api.Features.Projects.GetProjects;

public static class GetProjectsEndpoint
{
    public static void MapGetProjectsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/projects", [Authorize] async (IMediator mediator) =>
        {
            var result = await mediator.Send(new GetProjectsQuery());
            return Results.Ok(result);
        });
    }
}
