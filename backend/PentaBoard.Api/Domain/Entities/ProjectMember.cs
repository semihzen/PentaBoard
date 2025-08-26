// Domain/ProjectMember.cs
public class ProjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public string SubRole { get; set; } = "member";
    public Guid AddedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
