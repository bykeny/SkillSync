using SkillSync.Api.Data.Entities;

namespace SkillSync.Api.DTOs.Activities;

public class ActivityDto
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

    // Related skill info
    public int SkillId { get; set; }
    public string SkillName { get; set; } = string.Empty;

    // Computed properties
    public string TypeDisplay => Type.ToString();
    public string StatusDisplay => Status.ToString();
    public double DurationHours => DurationMinutes / 60.0;
}