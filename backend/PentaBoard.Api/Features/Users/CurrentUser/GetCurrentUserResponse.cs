namespace PentaBoard.Api.Features.Users.GetCurrentUser;

public record GetCurrentUserResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role,
    string Handle  // email'in @'ten önceki kısmı
);
