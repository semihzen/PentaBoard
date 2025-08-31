using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace PentaBoard.Api.Features.Files.UploadFiles;

public static class UploadProjectFileEndpoint
{
    public static IEndpointRouteBuilder MapUploadProjectFileEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/projects/{projectId:guid}/files",
            [Authorize] async (Guid projectId, IFormFile file, HttpContext http, IMediator mediator) =>
            {
                // Token'dan geçerli Guid çek; yoksa 401
                if (!TryGetUserId(http.User, out var userId))
                    return Results.Unauthorized();

                var dto = await mediator.Send(new UploadProjectFileCommand(projectId, file, userId));
                return Results.Created($"/api/projects/{projectId}/files/{dto.Id}", dto);
            })
           .DisableAntiforgery()
           .WithTags("ProjectFiles")
           .WithName("UploadProjectFile");

        return app;
    }

    // "sub", "id", "nameidentifier" sırasıyla dener; geçerli Guid bulursa döndürür
    static bool TryGetUserId(ClaimsPrincipal user, out Guid userId)
    {
        var val =
            user.FindFirst("sub")?.Value ??
            user.FindFirst("id")?.Value ??
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(val, out userId) && userId != Guid.Empty;
    }
}
