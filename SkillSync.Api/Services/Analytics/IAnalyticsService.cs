using SkillSync.Api.DTOs.Analytics;

namespace SkillSync.Api.Services.Analytics;

public interface IAnalyticsService
{
    Task<OverallStatsDto> GetOverallStatsAsync(string userId);
    Task<ProgressChartDto> GetProgressChartDataAsync(string userId, int days = 30);
    Task<SkillRadarDto> GetSkillRadarDataAsync(string userId);
    Task<ActivityTimelineDto> GetActivityTimelineAsync(string userId, int days = 30);
}