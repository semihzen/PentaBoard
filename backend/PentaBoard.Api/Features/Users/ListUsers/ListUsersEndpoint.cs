using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PentaBoard.Api.Features.Users.ListUsers;

[ApiController]
[Route("api/users")]
public class ListUsersEndpoint : ControllerBase
{
    private readonly ISender _mediator;
    public ListUsersEndpoint(ISender mediator) => _mediator = mediator;

    // Sadece adminlerin görmesi için iki rol adı örnek olarak eklendi.
    // Farklı adlar kullanıyorsan burayı güncelle (örn: "System Admin", "Admin")
    [Authorize(Roles = "Admin,System Admin")]
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ListUsersResponse>>> List(
        [FromQuery] string? q,
        [FromQuery] string? role = "all",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100)
    {
        var result = await _mediator.Send(new ListUsersQuery(q, role, page, pageSize));
        return Ok(result); // UsersPage doğrudan dizi bekliyor
    }
}
