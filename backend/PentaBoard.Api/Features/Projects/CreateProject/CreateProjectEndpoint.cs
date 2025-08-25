// Features/Projects/CreateProject/CreateProjectEndpoint.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace PentaBoard.Api.Features.Projects.CreateProject;

public static class CreateProjectEndpoint
{
    public static void MapCreateProjectEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/projects",
            [Authorize(Roles = "Admin,System Admin")] async (CreateProjectCommand cmd, IMediator mediator) =>
            {
                var result = await mediator.Send(cmd);
                return Results.Created($"/api/projects/{result.Id}", result);
            });
    }
}
