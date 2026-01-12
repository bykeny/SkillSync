namespace SkillSync.Web.Models;

public class ProgressChartData
{
    public List<string> Labels { get; set; } = new();
    public Dictionary<string, List<int>> Series { get; set; } = new();
}
