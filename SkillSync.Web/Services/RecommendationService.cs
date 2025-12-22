using System.Net.Http.Json;
using Blazored.LocalStorage;
using SkillSync.Web.Models;

namespace SkillSync.Web.Services;

public interface IRecommendationService
{
    Task<List<Recommendation>> GetAllRecommendationsAsync();
    Task<Recommendation?> GetRecommendationByIdAsync(int id);
    Task<string?> GenerateLearningPathAsync(int skillId);
    Task<string?> GenerateWeeklyScheduleAsync();
    Task<string?> GenerateSkillGapAnalysisAsync();
}

public class RecommendationService : IRecommendationService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public RecommendationService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<List<Recommendation>> GetAllRecommendationsAsync()
    {
        await SetAuthHeaderAsync();
        var recommendations = await _httpClient.GetFromJsonAsync<List<Recommendation>>("api/recommendations");
        return recommendations ?? new List<Recommendation>();
    }

    public async Task<Recommendation?> GetRecommendationByIdAsync(int id)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<Recommendation>($"api/recommendations/{id}");
    }

    public async Task<string?> GenerateLearningPathAsync(int skillId)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync($"api/recommendations/learning-path/{skillId}", null);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<RecommendationResponse>();
            return result?.Content;
        }
        
        return null;
    }

    public async Task<string?> GenerateWeeklyScheduleAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync("api/recommendations/weekly-schedule", null);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<RecommendationResponse>();
            return result?.Content;
        }
        
        return null;
    }

    public async Task<string?> GenerateSkillGapAnalysisAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync("api/recommendations/skill-gap-analysis", null);
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<RecommendationResponse>();
            return result?.Content;
        }
        
        return null;
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

public class RecommendationResponse
{
    public string Content { get; set; } = string.Empty;
}