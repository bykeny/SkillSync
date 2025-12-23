namespace SkillSync.Api.DTOs.Analytics;

public class SkillRadarDto
{
    public List<string> Labels { get; set; } = new(); // Skill names
    public List<int> CurrentLevels { get; set; } = new();
    public List<int> TargetLevels { get; set; } = new();
}