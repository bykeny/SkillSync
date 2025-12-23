namespace SkillSync.Api.DTOs.Analytics;

public class ProgressChartDto
{
    public List<string> Labels { get; set; } = new(); // Dates
    public Dictionary<string, List<int>> Series { get; set; } = new(); // Skill name -> proficiency levels
}