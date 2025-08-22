namespace PentaBoard.Api.Domain;

public class UserInvite
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public string Role { get; set; } = "User";
    public string? Note { get; set; }

    public Guid? InviterId { get; set; }  // opsiyonel: daveti yapan kullanıcı

    // Token’ı düz metin TUTMUYORUZ. Sadece hash’i saklıyoruz.
    public string TokenHash { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }  // UTC
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcceptedAt { get; set; }
    public bool IsUsed { get; set; } = false;
}