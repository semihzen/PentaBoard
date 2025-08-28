using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.WorkItems.Delete;

public static class DeleteWorkItemEndpoint
{
    // DELETE /api/projects/{projectId}/boards/{boardId}/workitems/{workItemId}
    public static IEndpointRouteBuilder MapDeleteWorkItem(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/projects/{projectId:guid}/boards/{boardId:guid}/workitems/{workItemId:guid}",
            async ([FromRoute] Guid projectId,
                   [FromRoute] Guid boardId,
                   [FromRoute] Guid workItemId,
                   ISender sender) =>
            {
                var ok = await sender.Send(new DeleteWorkItemCommand(projectId, boardId, workItemId));
                return ok ? Results.Ok(new { id = workItemId }) : Results.NotFound("Work item not found.");
            })
            .WithName("DeleteWorkItem")
            .WithTags("WorkItems")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
