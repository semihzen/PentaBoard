using MediatR;

namespace PentaBoard.Api.Features.Users.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<GetCurrentUserResponse>;
