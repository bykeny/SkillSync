namespace SkillSync.Api.Data.Entities;

public class SkillCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconName { get; set; } // For UI display
    public string? ColorHex { get; set; } // For UI display

    // Navigation properties
    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
}