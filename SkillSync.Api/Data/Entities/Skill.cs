using System.Diagnostics;

namespace SkillSync.Api.Data.Entities;

public class Skill
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProficiencyLevel { get; set; } // 1-5 scale
    public int TargetLevel { get; set; } = 5;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Foreign key
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public int? CategoryId { get; set; }
    public virtual SkillCategory? Category { get; set; }

    // Navigation properties
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public virtual ICollection<Milestone> Milestones { get; set; } = new List<Milestone>();
    public virtual ICollection<ProgressHistory> ProgressHistories { get; set; } = new List<ProgressHistory>();
}
