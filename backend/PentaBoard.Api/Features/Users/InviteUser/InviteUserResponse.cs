namespace PentaBoard.Api.Features.Users.InviteUser;

public record InviteUserResponse(
    Guid InviteId,
    string Email,
    string Role,
    DateTime ExpiresAt
);