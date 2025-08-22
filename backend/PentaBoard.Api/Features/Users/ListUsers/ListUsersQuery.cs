using MediatR;

namespace PentaBoard.Api.Features.Users.ListUsers;

public record ListUsersQuery(
    string? Q,
    string? Role,
    int Page = 1,
    int PageSize = 100
) : IRequest<IReadOnlyList<ListUsersResponse>>;
