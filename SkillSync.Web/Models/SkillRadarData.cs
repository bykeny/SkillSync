namespace SkillSync.Web.Models;

public class SkillRadarData
{
    public List<string> Labels { get; set; } = new();
    public List<int> CurrentLevels { get; set; } = new();
    public List<int> TargetLevels { get; set; } = new();
}
