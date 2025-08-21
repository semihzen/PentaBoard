using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.Authentication.LoginUser;

[ApiController]
[Route("api/auth")]
public class LoginUserEndpoint : ControllerBase
{
    private readonly ISender _mediator;
    public LoginUserEndpoint(ISender mediator) => _mediator = mediator;

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserCommand cmd)
    {
        try
        {
            var token = await _mediator.Send(cmd);
            return Ok(new { token });
        }
        catch
        {
            return Unauthorized();
        }
    }
}
