namespace SkillSync.Api.DTOs.Milestones;

public class MilestoneDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime TargetDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public int ProgressPercentage { get; set; }
    public DateTime CreatedAt { get; set; }

    // Related skill
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;

    // Computed properties
    public bool IsOverdue => !IsCompleted && TargetDate < DateTime.UtcNow;
    public int DaysRemaining => (TargetDate - DateTime.UtcNow).Days;
}