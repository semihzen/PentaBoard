// Features/Users/GetCurrentUser/GetCurrentUserHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace PentaBoard.Api.Features.Users.GetCurrentUser;

public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, GetCurrentUserResponse>
{
    private readonly AppDbContext _db;
    private readonly IHttpContextAccessor _http;

    public GetCurrentUserHandler(AppDbContext db, IHttpContextAccessor http)
    {
        _db = db;
        _http = http;
    }

    public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var principal = _http.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
            throw new UnauthorizedAccessException();

        // Hem raw ("sub","email") hem mapped (ClaimTypes.*) isimlerini dene
        string? userId =
            principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
            principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
            principal.FindFirst("sub")?.Value;

        string? email =
            principal.FindFirst(ClaimTypes.Email)?.Value ??
            principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value ??
            principal.FindFirst("email")?.Value ??
            principal.FindFirst("preferred_username")?.Value ??
            principal.FindFirst("upn")?.Value;

        var user = !string.IsNullOrWhiteSpace(userId)
            ? await _db.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId, ct)
            : (!string.IsNullOrWhiteSpace(email)
                ? await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct)
                : null);

        if (user is null)
            throw new UnauthorizedAccessException();

        return new GetCurrentUserResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role,
            user.Email.Split('@')[0] // handle
        );
    }
}
