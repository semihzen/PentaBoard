using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using PentaBoard.Api.Domain;
using PentaBoard.Api.Infrastructure;
using PentaBoard.Api.Infrastructure.Email;
using System.Security.Cryptography;
using System.Security.Claims;
using System.Text;

namespace PentaBoard.Api.Features.Users.InviteUser;

public class InviteUserHandler : IRequestHandler<InviteUserCommand, InviteUserResponse>
{
    private readonly AppDbContext _db;
    private readonly IEmailSender _email;
    private readonly IConfiguration _cfg;
    private readonly IHttpContextAccessor _http;

    public InviteUserHandler(
        AppDbContext db,
        IEmailSender email,
        IConfiguration cfg,
        IHttpContextAccessor http)
    {
        _db = db; _email = email; _cfg = cfg; _http = http;
    }

    public async Task<InviteUserResponse> Handle(InviteUserCommand req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email))
            throw new ArgumentException("Email is required.", nameof(req.Email));

        var email = req.Email.Trim().ToLowerInvariant();

        // Kullanıcı zaten var mı?
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new InvalidOperationException("User already exists.");

        // Aynı e-posta için daha önce gönderilmiş, kullanılmamış davetleri temizle
        var stale = await _db.UserInvites
            .Where(x => x.Email == email && !x.IsUsed)
            .ToListAsync(ct);
        if (stale.Count > 0) _db.UserInvites.RemoveRange(stale);

        // Token üret (base64url) ve hash'le
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = WebEncoders.Base64UrlEncode(tokenBytes);
        var tokenHash = Sha256Base64(token);

        // Davet eden bilgileri (JWT’den)
        Guid? inviterId = null;
        string? inviterEmail = null;
        var user = _http.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            var idStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(idStr, out var g)) inviterId = g;
            inviterEmail = user.FindFirst(ClaimTypes.Email)?.Value;
        }

        var invite = new UserInvite
        {
            Id        = Guid.NewGuid(),
            Email     = email,
            Role      = string.IsNullOrWhiteSpace(req.Role) ? "User" : req.Role.Trim(),
            Note      = string.IsNullOrWhiteSpace(req.Note) ? null : req.Note.Trim(),
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddHours(48),
            CreatedAt = DateTime.UtcNow,
            InviterId = inviterId,
            IsUsed    = false
        };

        _db.UserInvites.Add(invite);
        await _db.SaveChangesAsync(ct);

        // Accept link (ENV ve appsettings anahtarlarını esnek oku)
        var frontend =
            _cfg["Frontend:BaseUrl"] ??
            _cfg["App:FrontendBaseUrl"] ??
            _cfg["Frontend__BaseUrl"] ??
            "http://localhost:3000";

        var acceptUrl = $"{frontend.TrimEnd('/')}/accept-invite?token={token}";

        var safeNote = string.IsNullOrWhiteSpace(invite.Note)
            ? ""
            : $"<p><i>{System.Net.WebUtility.HtmlEncode(invite.Note)}</i></p>";

        var body = $@"
<h3>You are invited to PentaBoard</h3>
<p><b>Email:</b> {System.Net.WebUtility.HtmlEncode(email)}<br/>
<b>Role:</b> {System.Net.WebUtility.HtmlEncode(invite.Role)}</p>
{safeNote}
<p>Click to accept and set your password:<br/>
<a href=""{acceptUrl}"">{acceptUrl}</a></p>
<p>This link expires at <b>{invite.ExpiresAt:u}</b> (UTC).</p>";

        // Reply-To olarak davet edenin e-postasını geç (varsa)
        await _email.SendAsync(
            to: email,
            subject: "PentaBoard Invitation",
            htmlBody: body,
            replyTo: inviterEmail,         // IEmailSender'da replyTo desteği varsa
            ct: ct);

        return new InviteUserResponse(invite.Id, invite.Email, invite.Role, invite.ExpiresAt);
    }

    private static string Sha256Base64(string token)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(bytes);
    }
}
