namespace SkillSync.Api.Data.Entities;
public class Notification
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public string? ActionUrl { get; set; }

    // Foreign key
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
}

public enum NotificationType
{
    MilestoneReminder,
    WeeklySummary,
    GitHubSync,
    Achievement,
    SystemAlert
}