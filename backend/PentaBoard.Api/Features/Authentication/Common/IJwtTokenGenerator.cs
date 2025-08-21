using PentaBoard.Api.Domain.Entities;

namespace PentaBoard.Api.Features.Authentication.Common;

public interface IJwtTokenGenerator
{
    string Generate(User user);
}