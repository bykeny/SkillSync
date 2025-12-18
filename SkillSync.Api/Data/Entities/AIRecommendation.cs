namespace SkillSync.Api.Data.Entities;
public class AIRecommendation
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty; // JSON or HTML formatted content
    public RecommendationType Type { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RelatedSkillIds { get; set; } // Comma-separated skill IDs

    // Foreign key
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
}

public enum RecommendationType
{
    LearningPath,
    WeeklySchedule,
    TaskSuggestion,
    SkillGapAnalysis,
    ResourceRecommendation
}