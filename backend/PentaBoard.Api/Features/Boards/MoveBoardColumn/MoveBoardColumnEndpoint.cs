using MediatR;
using Microsoft.AspNetCore.Routing;

namespace PentaBoard.Api.Features.Boards.MoveBoardColumn;

public static class MoveBoardColumnEndpoint
{
    public sealed record MoveRequest(int TargetIndex);

    public static IEndpointRouteBuilder MapMoveBoardColumn(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/projects/{projectId:guid}/board/columns/{columnId:guid}/move",
            async (Guid projectId, Guid columnId, MoveRequest body, ISender sender) =>
            {
                try
                {
                    var list = await sender.Send(new MoveBoardColumnCommand(
                        projectId, columnId, body.TargetIndex
                    ));
                    return Results.Ok(new { columns = list });
                }
                catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
                catch (Exception ex)            { return Results.BadRequest(new { error = ex.Message }); }
            })
            .RequireAuthorization();

        return app;
    }
}
