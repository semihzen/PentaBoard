namespace PentaBoard.Api.Domain.Entities;

public class WorkItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    // Konum
    public Guid ProjectId { get; set; }
    public Guid BoardId { get; set; }
    public Guid BoardColumnId { get; set; }

    // İçerik
    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    // Sınıflandırma
    public string Type { get; set; } = "Task";   // Task|Bug|Story|Issue...
    public byte? Priority { get; set; }          // 1-4/1-5
    public byte? Severity { get; set; }          // Bug için opsiyonel

    // Atama
    public Guid ReporterId { get; set; }
    public Guid? AssigneeId { get; set; }

    // Tahmin/planlama
    public int? StoryPoints { get; set; }
    public decimal? OriginalEstimateHours { get; set; }
    public decimal? RemainingHours { get; set; }
    public decimal? CompletedHours { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }

    // Diğer
    public string? TagsJson { get; set; }        // ["frontend","api"]
    public int OrderKey { get; set; }            // sütun içi sıra (10,20,30…)
    public bool IsArchived { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
