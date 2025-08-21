using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PentaBoard.Api.Domain.Entities;
using PentaBoard.Api.Features.Authentication.Common;
using PentaBoard.Api.Infrastructure;

namespace PentaBoard.Api.Features.Authentication.LoginUser;

public class LoginUserHandler : IRequestHandler<LoginUserCommand, string>
{
    private readonly AppDbContext _db;
    private readonly IJwtTokenGenerator _jwt;
    private readonly PasswordHasher<User> _hasher = new();

    public LoginUserHandler(AppDbContext db, IJwtTokenGenerator jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    public async Task<string> Handle(LoginUserCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid credentials");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials");

        return _jwt.Generate(user);
    }
}