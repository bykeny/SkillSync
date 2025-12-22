using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.JSInterop;
using SkillSync.Web.Models;

namespace SkillSync.Web.Services;

public interface IGitHubService
{
    Task<GitHubConnection?> GetConnectionStatusAsync();
    Task<string?> GetAuthUrlAsync();
    Task<bool> CompleteConnectionAsync(string code);
    Task<bool> DisconnectAsync();
    Task<GitHubStats?> GetStatsAsync();
    Task<bool> SyncAsync();
}

public class GitHubService : IGitHubService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;

    public GitHubService(HttpClient httpClient, ILocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }

    public async Task<GitHubConnection?> GetConnectionStatusAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<GitHubConnection>("api/github/status");
    }

    public async Task<string?> GetAuthUrlAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.GetFromJsonAsync<GitHubAuthResponse>("api/github/connect");
        return response?.AuthUrl;
    }

    public async Task<bool> CompleteConnectionAsync(string code)
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsJsonAsync("api/github/complete-connection", new { code });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DisconnectAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync("api/github/disconnect", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<GitHubStats?> GetStatsAsync()
    {
        await SetAuthHeaderAsync();
        return await _httpClient.GetFromJsonAsync<GitHubStats>("api/github/stats");
    }

    public async Task<bool> SyncAsync()
    {
        await SetAuthHeaderAsync();
        var response = await _httpClient.PostAsync("api/github/sync", null);
        return response.IsSuccessStatusCode;
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

public class GitHubAuthResponse
{
    public string AuthUrl { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}