namespace PentaBoard.Api.Infrastructure.Email;

public class SmtpOptions
{
    public string Host { get; set; } = default!;
    public int Port { get; set; } = 587;
    public string User { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string FromAddress { get; set; } = default!;
    public string FromName { get; set; } = "PentaBoard";
    public bool UseStartTls { get; set; } = true;
}
