using System.Net.Http.Json;
using Blazored.LocalStorage;
using SkillSync.Web.Models;

namespace SkillSync.Web.Services;

public interface IAnalyticsService
{
    Task<OverallStats?> GetOverviewAsync();
    Task<object?> GetProgressChartAsync(int days = 30);
    Task<object?> GetSkillRadarAsync();
    Task<object?> GetActivityTimelineAsync(int days = 30);
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
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<OverallStats>("api/analytics/overview");
    }

    public async Task<object?> GetProgressChartAsync(int days = 30)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<object>($"api/analytics/progress-chart?days={days}");
    }

    public async Task<object?> GetSkillRadarAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<object>("api/analytics/skill-radar");
    }

    public async Task<object?> GetActivityTimelineAsync(int days = 30)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<object>($"api/analytics/activity-timeline?days={days}");
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