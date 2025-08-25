namespace PentaBoard.Api.Domain;

public class Project
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string? Description { get; set; }
    public string Color { get; set; } = "teal";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Tags { get; set; }

    // --- Ownership / Audit ---
    public Guid ProjectAdminId { get; set; }           // Zorunlu: projeden sorumlu System Admin
    public Guid CreatedById { get; set; }              // Projeyi oluşturan kullanıcı (Admin ya da System Admin)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Nav props (opsiyonel)
    public Entities.User? ProjectAdmin { get; set; }
    public Entities.User? CreatedBy { get; set; }
}
