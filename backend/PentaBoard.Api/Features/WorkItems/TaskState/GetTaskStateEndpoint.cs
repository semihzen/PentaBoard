// Features/WorkItems/TaskState/GetTaskStateEndpoint.cs
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.WorkItems.TaskState;

public static class GetTaskStateEndpoint
{
    // GET /api/projects/{projectId}/boards/{boardId}/workitems/taskstate?days=7
    public static IEndpointRouteBuilder MapGetTaskState(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/projects/{projectId:guid}/boards/{boardId:guid}/workitems/taskstate",
        async ([FromRoute] Guid projectId,
               [FromRoute] Guid boardId,
               [FromQuery] int days,
               ISender sender) =>
        {
            days = days <= 0 ? 7 : days;
            var dto = await sender.Send(new GetTaskStateQuery(projectId, boardId, days));
            return Results.Ok(dto);
        })
        .WithName("GetTaskState")
        .WithTags("WorkItems")
        .Produces<TaskStateDto>(StatusCodes.Status200OK);

        return app;
    }
}
