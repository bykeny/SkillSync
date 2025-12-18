namespace SkillSync.Api.DTOs.Milestones;

public class UpdateMilestoneDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime TargetDate { get; set; }
    public int ProgressPercentage { get; set; }
    public bool IsCompleted { get; set; }
}