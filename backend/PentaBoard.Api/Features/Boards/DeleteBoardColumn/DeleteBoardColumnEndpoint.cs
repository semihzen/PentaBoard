using MediatR;
using Microsoft.AspNetCore.Routing;

namespace PentaBoard.Api.Features.Boards.DeleteBoardColumn;

public static class DeleteBoardColumnEndpoint
{
    public static IEndpointRouteBuilder MapDeleteBoardColumn(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/projects/{projectId:guid}/board/columns/{columnId:guid}",
            async (Guid projectId, Guid columnId, ISender sender) =>
            {
                try
                {
                    var list = await sender.Send(new DeleteBoardColumnCommand(projectId, columnId));
                    return Results.Ok(new { columns = list });
                }
                catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
                catch (Exception ex)            { return Results.BadRequest(new { error = ex.Message }); }
            })
           .RequireAuthorization();

        return app;
    }
}
