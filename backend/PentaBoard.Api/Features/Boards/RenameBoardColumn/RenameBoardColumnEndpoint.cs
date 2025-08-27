using MediatR;
using Microsoft.AspNetCore.Routing;

namespace PentaBoard.Api.Features.Boards.RenameBoardColumn;

public static class RenameBoardColumnEndpoint
{
    public sealed record RenameBody(string Name);

    public static IEndpointRouteBuilder MapRenameBoardColumn(this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/projects/{projectId:guid}/board/columns/{columnId:guid}/name",
            async (Guid projectId, Guid columnId, RenameBody body, ISender sender) =>
            {
                try
                {
                    var dto = await sender.Send(new RenameBoardColumnCommand(projectId, columnId, body.Name));
                    return Results.Ok(dto);
                }
                catch (KeyNotFoundException ex) { return Results.NotFound(new { error = ex.Message }); }
                catch (Exception ex)            { return Results.BadRequest(new { error = ex.Message }); }
            })
           .RequireAuthorization();

        return app;
    }
}
