using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;

namespace PentaBoard.Api.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpOptions _opt;
    public SmtpEmailSender(IOptions<SmtpOptions> opt) => _opt = opt.Value;

   public async Task SendAsync(string to, string subject, string htmlBody, string? replyTo = null, CancellationToken ct = default)
{
    // ... FromAddress/Host/User/Password guard'ları eklemen iyi olur
    var msg = new MimeMessage();
    msg.From.Add(new MailboxAddress(_opt.FromName ?? _opt.FromAddress, _opt.FromAddress));
    msg.To.Add(MailboxAddress.Parse(to));
    if (!string.IsNullOrWhiteSpace(replyTo))
        msg.ReplyTo.Add(MailboxAddress.Parse(replyTo));

    msg.Subject = subject ?? "(no subject)";
    msg.Body = new BodyBuilder { HtmlBody = htmlBody ?? string.Empty }.ToMessageBody();

    using var client = new SmtpClient();
    await client.ConnectAsync(_opt.Host, _opt.Port, _opt.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, ct);
    await client.AuthenticateAsync(_opt.User, _opt.Password, ct);
    await client.SendAsync(msg, ct);
    await client.DisconnectAsync(true, ct);
}
}