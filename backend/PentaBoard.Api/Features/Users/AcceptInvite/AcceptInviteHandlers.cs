using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;                 // ⬅️ Identity hasher
using PentaBoard.Api.Infrastructure;                 // AppDbContext
using PentaBoard.Api.Domain;                         // UserInvite
using PentaBoard.Api.Domain.Entities;                // User
using System.Security.Cryptography;
using System.Text;

namespace PentaBoard.Api.Features.Users.AcceptInvite;

public class VerifyInviteHandler : IRequestHandler<VerifyInviteQuery, VerifyInviteResponse?>
{
    private readonly AppDbContext _db;
    public VerifyInviteHandler(AppDbContext db) => _db = db;

    public async Task<VerifyInviteResponse?> Handle(VerifyInviteQuery req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Token)) return null;

        static string Sha256Base64(string s)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(s)));
        }

        var hash = Sha256Base64(req.Token);

        var inv = await _db.UserInvites
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.TokenHash == hash && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow, ct);

        return inv is null
            ? null
            : new VerifyInviteResponse(inv.Email, inv.Role, inv.ExpiresAt, inv.Note);
    }
}

public class AcceptInviteHandler : IRequestHandler<AcceptInviteCommand, Guid>
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _hasher = new();   // ⬅️ ASP.NET Identity hasher (PBKDF2)

    public AcceptInviteHandler(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> Handle(AcceptInviteCommand req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Token))
            throw new ArgumentException("Token is required.", nameof(req.Token));
        if (string.IsNullOrWhiteSpace(req.FirstName) || string.IsNullOrWhiteSpace(req.LastName))
            throw new ArgumentException("First/last name is required.");
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters.");

        var hash = Sha256Base64(req.Token);

        var inv = await _db.UserInvites.FirstOrDefaultAsync(i => i.TokenHash == hash, ct);
        if (inv is null || inv.IsUsed || inv.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Invite is invalid or expired.");

        // Aynı email’le kullanıcı zaten varsa
        var normalizedEmail = inv.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == normalizedEmail, ct))
            throw new InvalidOperationException("User already exists.");

        var user = new User
        {
            Id          = Guid.NewGuid(),
            Email       = normalizedEmail,
            FirstName   = req.FirstName.Trim(),
            LastName    = req.LastName.Trim(),
            Role        = inv.Role
        };

        // ⬅️ Identity ile hash’le (Login tarafıyla aynı algoritma)
        user.PasswordHash = _hasher.HashPassword(user, req.Password);

        _db.Users.Add(user);

        // Bu daveti kullanıldı olarak işaretle
        inv.IsUsed     = true;
        inv.AcceptedAt = DateTime.UtcNow;

        // Aynı email için diğer açık davetleri de kapat
        var others = _db.UserInvites.Where(i => i.Email == normalizedEmail && !i.IsUsed && i.Id != inv.Id);
        await others.ForEachAsync(o => { o.IsUsed = true; o.AcceptedAt = DateTime.UtcNow; }, ct);

        await _db.SaveChangesAsync(ct);
        return user.Id;
    }

    private static string Sha256Base64(string s)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(s)));
    }
}
