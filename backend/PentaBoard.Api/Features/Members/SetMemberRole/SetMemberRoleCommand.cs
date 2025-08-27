using MediatR;

namespace PentaBoard.Api.Features.Members.SetMemberRole;

public sealed record SetMemberRoleCommand(
    Guid ProjectId,
    Guid TargetUserId,
    string NewSubRole
) : IRequest<Unit>;
