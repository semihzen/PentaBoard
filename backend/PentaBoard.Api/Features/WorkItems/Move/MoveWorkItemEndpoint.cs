using MediatR;
using Microsoft.AspNetCore.Routing;

namespace PentaBoard.Api.Features.WorkItems.Move;

public static class MoveWorkItemEndpoint
{
    // Extension method – Program.cs içinde app.MapMoveWorkItemEndpoint() diye çağıracaksın
    public static IEndpointRouteBuilder MapMoveWorkItemEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/projects/{projectId:guid}/boards/{boardId:guid}/workitems/{id:guid}/move",
            async (Guid projectId, Guid boardId, Guid id, MoveWorkItemCommand body, IMediator mediator) =>
            {
                // route/body güvenliği
                if (id != body.WorkItemId || projectId != body.ProjectId || boardId != body.BoardId)
                    return Results.BadRequest("Route and body mismatch.");

                var dto = await mediator.Send(body);
                return Results.Ok(dto);
            })
            .WithTags("WorkItems")
            .WithName("MoveWorkItem");

        return app;
    }
}
