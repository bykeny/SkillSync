namespace SkillSync.Api.Data.Entities;

public class ProgressHistory
{
    public int Id { get; set; }
    public int PreviousLevel { get; set; }
    public int NewLevel { get; set; }
    public string? Notes { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    // Foreign key
    public int SkillId { get; set; }
    public virtual Skill Skill { get; set; } = null!;
}
