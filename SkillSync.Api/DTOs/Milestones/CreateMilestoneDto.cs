namespace SkillSync.Api.DTOs.Milestones;

public class CreateMilestoneDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime TargetDate { get; set; }
    public int SkillId { get; set; }
}