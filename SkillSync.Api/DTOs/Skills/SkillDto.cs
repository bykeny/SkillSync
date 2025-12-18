namespace SkillSync.Api.DTOs.Skills;

public class SkillDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProficiencyLevel { get; set; }
    public int TargetLevel { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryColor { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public int ProgressPercentage => TargetLevel > 0 ? (ProficiencyLevel * 100) / TargetLevel : 0;
}