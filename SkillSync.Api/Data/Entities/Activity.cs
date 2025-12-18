namespace SkillSync.Api.Data.Entities;

public class Activity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActivityType Type { get; set; } // Course, Project, Reading, Practice, etc.
    public ActivityStatus Status { get; set; } = ActivityStatus.NotStarted;
    public int DurationMinutes { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? ResourceUrl { get; set; }
    public string? Notes { get; set; }

    // Foreign keys
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    public int SkillId { get; set; }
    public virtual Skill Skill { get; set; } = null!;
}

public enum ActivityType
{
    Course,
    Project,
    Reading,
    Practice,
    Video,
    Workshop,
    Certification,
    Other
}

public enum ActivityStatus
{
    NotStarted,
    InProgress,
    Completed,
    Paused,
    Cancelled
}