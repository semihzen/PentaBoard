namespace PentaBoard.Api.Domain.Entities;

public class Board
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }

    public string Name { get; set; } = "Default";
    public Guid CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Opsiyonel referanslar (seed sonrası set edilebilir)
    public Guid? DefaultColumnId { get; set; }
    public Guid? DoneColumnId { get; set; }

    // UI ayarları (opsiyonel)
    public string? SettingsJson { get; set; }

    public ICollection<BoardColumn> Columns { get; set; } = new List<BoardColumn>();
}
