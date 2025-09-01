// Features/WorkItems/Get/GetWorkItemByIdEndpoint.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PentaBoard.Api.Features.WorkItems.Common;

namespace PentaBoard.Api.Features.WorkItems.Get;

public static class GetWorkItemByIdEndpoint
{
    // GET /api/projects/{projectId}/boards/{boardId}/workitems/{workItemId}
    public static IEndpointRouteBuilder MapGetWorkItemById(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/projects/{projectId:guid}/boards/{boardId:guid}/workitems/{workItemId:guid}",
            async ([FromRoute] Guid projectId,
                   [FromRoute] Guid boardId,
                   [FromRoute] Guid workItemId,
                   ISender sender) =>
            {
                var dto = await sender.Send(new GetWorkItemByIdQuery(projectId, boardId, workItemId));
                return dto is null ? Results.NotFound("Work item not found.") : Results.Ok(dto);
            })
           .WithName("GetWorkItemById")
           .WithTags("WorkItems")
           .Produces<WorkItemDto>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);

        return app;
    }
}
