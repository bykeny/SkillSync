using SkillSync.Api.Data.Entities;

namespace SkillSync.Api.DTOs.Activities;

public class UpdateActivityDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ActivityType Type { get; set; }
    public ActivityStatus Status { get; set; }
    public int DurationMinutes { get; set; }
    public int SkillId { get; set; }
    public string? ResourceUrl { get; set; }
    public string? Notes { get; set; }
}