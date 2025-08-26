using MediatR;

namespace PentaBoard.Api.Features.Members.AddProjectMember;

public sealed record AddProjectMemberCommand(
    Guid ProjectId,
    Guid UserId,
    string SubRole
) : IRequest<Unit>;
