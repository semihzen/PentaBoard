using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace PentaBoard.Api.Features.Projects.UpdateProject;

public static class UpdateProjectEndpoint
{
    public static void MapUpdateProjectEndpoint(this IEndpointRouteBuilder app)
    {
        // Endpoint yalın; yalnızca komutu IMediator’a iletir
        app.MapPost("/api/projects/{id:guid}", [Authorize] async (
            Guid id, UpdateProjectRequest body, IMediator mediator) =>
        {
            var cmd = new UpdateProjectCommand(
                id,
                body.Description,
                body.StartDate,
                body.EndDate,
                body.Tags
            );

            var result = await mediator.Send(cmd);
            return Results.Ok(result);
        });
    }

    // İstek DTO’su (isteğe bağlı alanlar)
    public sealed class UpdateProjectRequest
    {
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Tags { get; set; }
    }
}
