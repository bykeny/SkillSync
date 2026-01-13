using Microsoft.EntityFrameworkCore;
using SkillSync.Api.Data;
using SkillSync.Api.Services.GitHub;

namespace SkillSync.Api.Services.BackgroundJobs;

public class GitHubSyncJob
{
    private readonly ApplicationDbContext _context;
    private readonly IGitHubService _gitHubService;
    private readonly ILogger<GitHubSyncJob> _logger;

    public GitHubSyncJob(
        ApplicationDbContext context,
        IGitHubService gitHubService,
        ILogger<GitHubSyncJob> logger)
    {
        _context = context;
        _gitHubService = gitHubService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting GitHub sync job");

        var connectedProfiles = await _context.GitHubProfiles
            .Where(p => p.IsActive && !string.IsNullOrEmpty(p.AccessToken))
            .ToListAsync();

        foreach (var profile in connectedProfiles)
        {
            try
            {
                await _gitHubService.SyncGitHubDataAsync(profile.UserId);
                _logger.LogInformation("GitHub data synced for user {UserId}", profile.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to sync GitHub data for user {UserId}", profile.UserId);
            }
        }

        _logger.LogInformation("GitHub sync job completed. Synced {Count} profiles", connectedProfiles.Count);
    }
}