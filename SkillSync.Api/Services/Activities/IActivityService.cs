using SkillSync.Api.DTOs.Activities;

namespace SkillSync.Api.Services.Activities;

public interface IActivityService
{
    Task<IEnumerable<ActivityDto>> GetAllActivitiesAsync(string userId);
    Task<ActivityDto?> GetActivityByIdAsync(int id, string userId);
    Task<ActivityDto> CreateActivityAsync(CreateActivityDto dto, string userId);
    Task<ActivityDto?> UpdateActivityAsync(int id, UpdateActivityDto dto, string userId);
    Task<bool> DeleteActivityAsync(int id, string userId);
    Task<IEnumerable<ActivityDto>> GetActivitiesBySkillAsync(int skillId, string userId);
    Task<ActivityDto?> StartActivityAsync(int id, string userId);
    Task<ActivityDto?> CompleteActivityAsync(int id, string userId);
    Task<ActivityStatsDto> GetActivityStatsAsync(string userId);
}