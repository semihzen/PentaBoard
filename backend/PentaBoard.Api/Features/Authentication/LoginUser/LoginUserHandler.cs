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
        // 🔧 ÖNEMLİ: normalize et
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (user is null)
            throw new UnauthorizedAccessException("Invalid credentials");

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);

        // İsteğe bağlı: SuccessRehashNeeded gelirse hash'i yenile
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            await _db.SaveChangesAsync(ct);
        }

        if (result == PasswordVerificationResult.Failed)
            throw new UnauthorizedAccessException("Invalid credentials");

        return _jwt.Generate(user);
    }
}
