using System.Threading;
using System.Threading.Tasks;

namespace PentaBoard.Api.Infrastructure.Email;

public interface IEmailSender
{
    Task SendAsync(string to, string subject, string htmlBody, string? replyTo = null, CancellationToken ct = default);
}