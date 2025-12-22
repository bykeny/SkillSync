namespace SkillSync.Web.Models;

public class Recommendation
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public bool IsActive { get; set; }
}