using MediatR;

namespace PentaBoard.Api.Features.Authentication.LoginUser;

public record LoginUserCommand(string Email, string Password) : IRequest<string>;