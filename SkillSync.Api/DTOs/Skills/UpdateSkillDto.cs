namespace SkillSync.Api.DTOs.Skills;

public class UpdateSkillDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProficiencyLevel { get; set; }
    public int TargetLevel { get; set; }
    public int? CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
}