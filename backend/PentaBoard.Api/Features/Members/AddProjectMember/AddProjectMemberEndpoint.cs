using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.Members.AddProjectMember;

public static class AddProjectMemberEndpoint
{
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/projects/{id:guid}/members",
            [Authorize] async ([FromRoute] Guid id, [FromBody] AddRequest body, ISender mediator) =>
            {
                await mediator.Send(new AddProjectMemberCommand(
                    ProjectId: id,
                    UserId: body.UserId,
                    SubRole: body.SubRole ?? "member"
                ));
                return Results.Ok();
            });

        return app;
    }

    public sealed class AddRequest
    {
        public Guid UserId { get; set; }
        public string? SubRole { get; set; }
    }
}
