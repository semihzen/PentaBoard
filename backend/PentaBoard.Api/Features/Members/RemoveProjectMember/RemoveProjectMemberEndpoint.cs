using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.Members.RemoveProjectMember;

public static class RemoveProjectMemberEndpoint
{
    // DELETE /api/projects/{id}/members/{userId}
    public static IEndpointRouteBuilder Map(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/projects/{id:guid}/members/{userId:guid}",
            [Authorize] async ([FromRoute] Guid id, [FromRoute] Guid userId, ISender mediator) =>
            {
                await mediator.Send(new RemoveProjectMemberCommand(id, userId));
                return Results.Ok();
            });

        return app;
    }
}
