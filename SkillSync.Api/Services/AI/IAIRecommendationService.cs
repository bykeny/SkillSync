using SkillSync.Api.DTOs.AI;

namespace SkillSync.Api.Services.AI;

public interface IAIRecommendationService
{
    Task<string> GenerateLearningPathAsync(string userId, int skillId);
    Task<string> GenerateWeeklyScheduleAsync(string userId);
    Task<string> GenerateSkillGapAnalysisAsync(string userId);
    Task<List<RecommendationDto>> GetUserRecommendationsAsync(string userId);
    Task<RecommendationDto?> GetRecommendationByIdAsync(int id, string userId);
}