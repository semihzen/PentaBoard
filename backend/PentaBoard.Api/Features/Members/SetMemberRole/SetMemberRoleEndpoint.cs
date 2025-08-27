using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.Members.SetMemberRole;

public static class SetMemberRoleEndpoint
{
    // PUT /api/projects/{id}/members/{userId}/role
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/projects/{id:guid}/members/{userId:guid}/role",
            [Authorize] async ([FromRoute] Guid id, [FromRoute] Guid userId, [FromBody] Body body, ISender mediator) =>
            {
                await mediator.Send(new SetMemberRoleCommand(
                    ProjectId: id,
                    TargetUserId: userId,
                    NewSubRole: body?.SubRole ?? "member"
                ));
                return Results.Ok();
            })
            .WithTags("Members");

        return app;
    }

    public sealed class Body
    {
        public string? SubRole { get; set; }
    }
}
