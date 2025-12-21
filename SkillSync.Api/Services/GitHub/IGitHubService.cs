using SkillSync.Api.DTOs.GitHub;

namespace SkillSync.Api.Services.GitHub;

public interface IGitHubService
{
    Task<string> GetAuthorizationUrlAsync(string state);
    Task<bool> HandleCallbackAsync(string code, string userId);
    Task<GitHubConnectionDto> GetConnectionStatusAsync(string userId);
    Task<bool> DisconnectAsync(string userId);
    Task<GitHubStatsDto> GetGitHubStatsAsync(string userId);
    Task SyncGitHubDataAsync(string userId);
    Task AutoMapLanguagesToSkillsAsync(string userId);
}