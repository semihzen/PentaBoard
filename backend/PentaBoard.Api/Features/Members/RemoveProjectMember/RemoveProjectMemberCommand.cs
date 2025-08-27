using MediatR;

namespace PentaBoard.Api.Features.Members.RemoveProjectMember;

public sealed record RemoveProjectMemberCommand(
    Guid ProjectId,
    Guid TargetUserId
) : IRequest<Unit>;
