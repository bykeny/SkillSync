namespace SkillSync.Api.DTOs.AI;

public class LearningPathDto
{
    public string SkillName { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public int TargetLevel { get; set; }
    public List<string> RecommendedSteps { get; set; } = new();
    public List<string> Resources { get; set; } = new();
    public string Timeline { get; set; } = string.Empty;
}