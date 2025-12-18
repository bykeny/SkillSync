namespace SkillSync.Api.Data.Entities;

public class Milestone
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; } = false;
    public int ProgressPercentage { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Foreign key
    public int SkillId { get; set; }
    public virtual Skill Skill { get; set; } = null!;
}