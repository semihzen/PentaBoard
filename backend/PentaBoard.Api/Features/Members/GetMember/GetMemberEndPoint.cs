using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Members.GetMember;

public static class GetMemberEndPoint
{
    // GET /api/projects/{projectId}/members
    public static IEndpointConventionBuilder MapGetProjectMembers(this IEndpointRouteBuilder app)
    {
        return app.MapGet("/api/projects/{projectId:guid}/members",
            async (Guid projectId, AppDbContext db, HttpContext ctx) =>
            {
                // Endpoint sadece kapı – kimlik çöz, handler’a pasla
                var userId = GetUserId(ctx.User);
                if (userId == Guid.Empty) return Results.Unauthorized();

                var handler = new GetMemberHandler(db);
                return await handler.Handle(
                    new GetMemberCommand(projectId, userId),
                    ctx.RequestAborted
                );
            })
            .WithTags("Members")
            .WithName("GetProjectMembers");
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var id =
            user.FindFirstValue("sub") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue("id");

        return Guid.TryParse(id, out var gid) ? gid : Guid.Empty;
    }
}
