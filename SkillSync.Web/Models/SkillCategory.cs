namespace SkillSync.Web.Models;

public class SkillCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconName { get; set; }
    public string? ColorHex { get; set; }
    public int SkillCount { get; set; }
}