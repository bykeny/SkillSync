using System.Net.Http.Json;
using Blazored.LocalStorage;
using SkillSync.Web.Models;

namespace SkillSync.Web.Services;

public interface ISkillService
{
    Task<List<Skill>> GetAllSkillsAsync();
    Task<Skill?> GetSkillByIdAsync(int id);
    Task<Skill?> CreateSkillAsync(CreateSkillRequest request);
    Task<Skill?> UpdateSkillAsync(int id, UpdateSkillRequest request);
    Task<bool> DeleteSkillAsync(int id);
    Task<List<SkillCategory>> GetCategoriesAsync();
}

public class SkillService : ISkillService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;

    public SkillService(HttpClient httpClient, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public async Task<List<Skill>> GetAllSkillsAsync()
    {
        await SetAuthHeaderAsync();
        var skills = await _httpClient.GetFromJsonAsync<List<Skill>>("api/skills");
        return skills ?? new List<Skill>();
    }

    public async Task<Skill?> GetSkillByIdAsync(int id)
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<Skill>($"api/skills/{id}");
    }

    public async Task<Skill?> CreateSkillAsync(CreateSkillRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/skills", request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Skill>();

        return null;
    }

    public async Task<Skill?> UpdateSkillAsync(int id, UpdateSkillRequest request)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PutAsJsonAsync($"api/skills/{id}", request);

        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<Skill>();

        return null;
    }

    public async Task<bool> DeleteSkillAsync(int id)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/skills/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<List<SkillCategory>> GetCategoriesAsync()
    {
        var categories = await _httpClient.GetFromJsonAsync<List<SkillCategory>>("api/skills/categories");
        return categories ?? new List<SkillCategory>();
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