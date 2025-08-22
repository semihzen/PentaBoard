// Features/Users/InviteUser/InviteUserEndpoint.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.Users.InviteUser;

[ApiController]
[Route("api/users")]
public class InviteUserEndpoint : ControllerBase
{
    private readonly ISender _mediator;
    public InviteUserEndpoint(ISender mediator) => _mediator = mediator;

    // ❗ Artık sadece "Admin" rolü davet gönderebilir
    [Authorize(Roles = "Admin")]
    [HttpPost("invite")]
    public async Task<ActionResult<InviteUserResponse>> Invite([FromBody] InviteUserCommand cmd)
    {
        // ApiController attribute'u model validation yapıyor, yine de net bir dönüş verelim
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var result = await _mediator.Send(cmd);
        return Ok(result);
    }
}
