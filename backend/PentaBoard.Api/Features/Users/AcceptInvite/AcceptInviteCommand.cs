using MediatR;

namespace PentaBoard.Api.Features.Users.AcceptInvite;

public record AcceptInviteCommand(
    string Token,
    string FirstName,
    string LastName,
    string Password
) : IRequest<Guid>; // Created UserId