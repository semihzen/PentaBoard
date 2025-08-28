using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.WorkItems.Create;

public static class CreateWorkItemEndpoint
{
    public static IEndpointRouteBuilder MapCreateWorkItem(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/projects/{projectId:guid}/boards/{boardId:guid}/workitems",
            async ([FromRoute] Guid projectId,
                   [FromRoute] Guid boardId,
                   [FromBody] CreateWorkItemBody body,
                   ISender sender) =>
            {
                var dto = await sender.Send(new CreateWorkItemCommand(
                    projectId,
                    boardId,
                    body.ColumnId,
                    body.Title,
                    body.Description,
                    body.Type,
                    body.Priority,
                    body.AssigneeId   // assignee desteği
                ));

                return Results.Ok(dto);
            })
            .WithName("CreateWorkItem")
            .WithTags("WorkItems")
            .Produces(StatusCodes.Status200OK);

        return app;
    }

    public sealed class CreateWorkItemBody
    {
        public Guid? ColumnId { get; set; }      // gönderilmezse default/ilk kolona düşer
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string Type { get; set; } = "Task";
        public byte? Priority { get; set; }
        public Guid? AssigneeId { get; set; }    // yeni: atanan kişi
    }
}
