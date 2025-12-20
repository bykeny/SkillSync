namespace SkillSync.Api.DTOs.AI;

public class GenerateRecommendationRequest
{
    public string Type { get; set; } = string.Empty; // "LearningPath", "WeeklySchedule", "SkillGap"
    public List<int>? SkillIds { get; set; }
}