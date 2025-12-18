namespace SkillSync.Web.Models;

public class CreateSkillRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProficiencyLevel { get; set; } = 1;
    public int TargetLevel { get; set; } = 5;
    public int? CategoryId { get; set; }
}