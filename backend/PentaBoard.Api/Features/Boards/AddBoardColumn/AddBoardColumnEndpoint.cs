using MediatR;
using Microsoft.AspNetCore.Routing;

namespace PentaBoard.Api.Features.Boards.AddBoardColumn;

public static class AddBoardColumnEndpoint
{
    public sealed record AddBoardColumnRequest(
        string Name,
        string? Color,
        int? WipLimit,
        bool? IsDoneLike,
        Guid? InsertAfterColumnId
    );

    public static IEndpointRouteBuilder MapAddBoardColumn(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/projects/{projectId:guid}/board/columns",
            async (Guid projectId, AddBoardColumnRequest body, ISender sender) =>
            {
                try
                {
                    var dto = await sender.Send(new AddBoardColumnCommand(
                        projectId,
                        body.Name,
                        body.Color,
                        body.WipLimit,
                        body.IsDoneLike,
                        body.InsertAfterColumnId
                    ));

                    return Results.Created($"/api/projects/{projectId}/board/columns/{dto.Id}", dto);
                }
                catch (InvalidOperationException ex)
                {
                    // 409: aynı isim vb.
                    return Results.Conflict(new { error = ex.Message });
                }
            })
            .RequireAuthorization(); // auth gerekli
        return app;
    }
}
