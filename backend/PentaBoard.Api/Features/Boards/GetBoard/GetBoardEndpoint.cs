using MediatR;
using Microsoft.AspNetCore.Routing;

namespace PentaBoard.Api.Features.Boards.GetBoard;

public static class GetBoardEndpoint
{
    public static IEndpointRouteBuilder MapGetBoard(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/projects/{projectId:guid}/board", async (Guid projectId, ISender sender) =>
        {
            var dto = await sender.Send(new GetBoardQuery(projectId));
            return Results.Ok(dto);
        });

        return app;
    }
}
