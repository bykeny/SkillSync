using System.Net.Http.Json;
using Blazored.LocalStorage;
using SkillSync.Web.Models;

namespace SkillSync.Web.Services;

public interface IAnalyticsService
{
    Task<OverallStats?> GetOverviewAsync();
    Task<ProgressChartData?> GetProgressChartAsync(int days = 30);
    Task<SkillRadarData?> GetSkillRadarAsync();
    Task<ActivityTimelineData?> GetActivityTimelineAsync(int days = 30);
}

public class AnalyticsService : IAnalyticsService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public AnalyticsService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<OverallStats?> GetOverviewAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<OverallStats>("api/analytics/overview");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<ProgressChartData?> GetProgressChartAsync(int days = 30)
    {
        try
        {
            await SetAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<ProgressChartData>($"api/analytics/progress-chart?days={days}");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<SkillRadarData?> GetSkillRadarAsync()
    {
        try
        {
            await SetAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<SkillRadarData>("api/analytics/skill-radar");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<ActivityTimelineData?> GetActivityTimelineAsync(int days = 30)
    {
        try
        {
            await SetAuthHeaderAsync();
            return await _httpClient.GetFromJsonAsync<ActivityTimelineData>($"api/analytics/activity-timeline?days={days}");
        }
        catch (Exception)
        {
            return null;
        }
    }

    private async Task SetAuthHeaderAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("accessToken");
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}