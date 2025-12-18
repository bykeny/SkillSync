using System.Net.Http.Json;
using Blazored.LocalStorage;
using SkillSync.Web.Models;

namespace SkillSync.Web.Services;

public interface IActivityService
{
    Task<List<Activity>> GetAllActivitiesAsync();
    Task<Activity?> GetActivityByIdAsync(int id);
    Task<Activity?> CreateActivityAsync(CreateActivityRequest request);
    Task<Activity?> UpdateActivityAsync(int id, UpdateActivityRequest request);
    Task<bool> DeleteActivityAsync(int id);
    Task<Activity?> StartActivityAsync(int id);
    Task<Activity?> CompleteActivityAsync(int id);
    Task<ActivityStats?> GetStatsAsync();
}

public class ActivityService : IActivityService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public ActivityService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<List<Activity>> GetAllActivitiesAsync()
    {
        await SetAuthHeaderAsync();
        var activities = await _httpClient.GetFromJsonAsync<List<Activity>>("api/activities");
        return activities ?? new List<Activity>();
    }

    public async Task<Activity?> GetActivityByIdAsync(int id)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<Activity>($"api/activities/{id}");
    }

    public async Task<Activity?> CreateActivityAsync(CreateActivityRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/activities", request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Activity>();

        return null;
    }

    public async Task<Activity?> UpdateActivityAsync(int id, UpdateActivityRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/activities/{id}", request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Activity>();

        return null;
    }

    public async Task<bool> DeleteActivityAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/activities/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<Activity?> StartActivityAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/activities/{id}/start", null);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Activity>();

        return null;
    }

    public async Task<Activity?> CompleteActivityAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/activities/{id}/complete", null);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Activity>();

        return null;
    }

    public async Task<ActivityStats?> GetStatsAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<ActivityStats>("api/activities/stats");
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