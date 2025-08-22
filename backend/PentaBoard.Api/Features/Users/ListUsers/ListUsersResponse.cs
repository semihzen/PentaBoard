namespace PentaBoard.Api.Features.Users.ListUsers;

public record ListUsersResponse(
    Guid Id,
    string? FirstName,
    string? LastName,
    string Email,
    string? Role
);
