using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.Users.AcceptInvite;

[ApiController]
[Route("api/users/invite")]
public class AcceptInviteEndpoint : ControllerBase
{
    private readonly ISender _mediator;
    public AcceptInviteEndpoint(ISender mediator) => _mediator = mediator;

    // VERIFY -> JSON: { email, role, expiresAt, note }
    [AllowAnonymous]
    [HttpGet("verify")]
    public async Task<IActionResult> Verify([FromQuery] string token)
    {
        var dto = await _mediator.Send(new VerifyInviteQuery(token));
        return dto is null
            ? NotFound(new { error = "Invite not found or expired" })
            : Ok(dto);
    }

    // ACCEPT -> handler Guid döndürüyorsa (oluşan kullanıcının Id'si)
    [AllowAnonymous]
    [HttpPost("accept")]
    public async Task<IActionResult> Accept([FromBody] AcceptInviteCommand cmd)
    {
        var userId = await _mediator.Send(cmd); // Guid bekliyoruz
        if (userId == Guid.Empty)
            return BadRequest("Invalid or expired invite.");

        return Ok(new { userId });
    }
}
