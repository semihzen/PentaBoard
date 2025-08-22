using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.Users.GetCurrentUser;

[ApiController]
[Route("api/users")]
public class GetCurrentUserEndpoint : ControllerBase
{
    private readonly ISender _mediator;
    public GetCurrentUserEndpoint(ISender mediator) => _mediator = mediator;

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<GetCurrentUserResponse>> Me()
        => Ok(await _mediator.Send(new GetCurrentUserQuery()));
}
