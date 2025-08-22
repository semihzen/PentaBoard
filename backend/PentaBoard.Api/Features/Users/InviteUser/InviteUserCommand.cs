using MediatR;

namespace PentaBoard.Api.Features.Users.InviteUser;

public record InviteUserCommand(
    string Email,
    string Role,
    string? Note
) : IRequest<InviteUserResponse>;