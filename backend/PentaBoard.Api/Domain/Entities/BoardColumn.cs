namespace PentaBoard.Api.Domain.Entities;

public class BoardColumn
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BoardId { get; set; }

    public string Name { get; set; } = null!;
    public int OrderKey { get; set; }          // 10,20,30...
    public string? Color { get; set; }
    public int? WipLimit { get; set; }
    public bool IsDefault { get; set; }
    public bool IsDoneLike { get; set; }
}
