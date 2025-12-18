namespace SkillSync.Web.Models;

public class Activity
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActivityType Type { get; set; }
    public ActivityStatus Status { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? ResourceUrl { get; set; }
    public string? Notes { get; set; }
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string TypeDisplay { get; set; } = string.Empty;
    public string StatusDisplay { get; set; } = string.Empty;
    public double DurationHours { get; set; }
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