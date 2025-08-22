using MediatR;

namespace PentaBoard.Api.Features.Users.AcceptInvite;

// bool yerine DTO
public record VerifyInviteQuery(string Token) : IRequest<VerifyInviteResponse?>;

public record VerifyInviteResponse(
    string Email,
    string Role,
    DateTime ExpiresAt,
    string? Note
);