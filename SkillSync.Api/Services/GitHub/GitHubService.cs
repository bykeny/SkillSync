using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Octokit;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.DTOs.GitHub;

namespace SkillSync.Api.Services.GitHub;

public class GitHubService : IGitHubService
{
    private readonly ApplicationDbContext _context;
    private readonly GitHubSettings _settings;
    private readonly ILogger<GitHubService> _logger;
    private readonly HttpClient _httpClient;

    public GitHubService(
        ApplicationDbContext context,
        IOptions<GitHubSettings> settings,
        ILogger<GitHubService> logger,
        HttpClient httpClient)
    {
        _context = context;
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClient;
    }

    public Task<string> GetAuthorizationUrlAsync(string state)
    {
        var url = $"https://github.com/login/oauth/authorize" +
                  $"?client_id={_settings.ClientId}" +
                  $"&redirect_uri={Uri.EscapeDataString(_settings.RedirectUri)}" +
                  $"&scope=repo,user" +
                  $"&state={state}";

        return Task.FromResult(url);
    }

    public async Task<bool> HandleCallbackAsync(string code, string userId)
    {
        try
        {
            // Exchange code for access token
            var tokenResponse = await ExchangeCodeForTokenAsync(code);

            if (string.IsNullOrEmpty(tokenResponse))
                return false;

            // Get GitHub user info
            var client = new GitHubClient(new ProductHeaderValue("SkillSync"))
            {
                Credentials = new Credentials(tokenResponse)
            };

            var user = await client.User.Current();

            // Save or update GitHub profile
            var existingProfile = await _context.GitHubProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (existingProfile != null)
            {
                existingProfile.AccessToken = tokenResponse;
                existingProfile.GitHubUsername = user.Login;
                existingProfile.IsActive = true;
            }
            else
            {
                var profile = new Data.Entities.GitHubProfile
                {
                    UserId = userId,
                    GitHubUsername = user.Login,
                    AccessToken = tokenResponse,
                    ConnectedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.GitHubProfiles.Add(profile);
            }

            await _context.SaveChangesAsync();

            // Update user's GitHub username
            var appUser = await _context.Users.FindAsync(userId);
            if (appUser != null)
            {
                appUser.GitHubUsername = user.Login;
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("GitHub connected for user {UserId}: {Username}", userId, user.Login);

            // Trigger initial sync
            await SyncGitHubDataAsync(userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GitHub callback");
            return false;
        }
    }

    public async Task<GitHubConnectionDto> GetConnectionStatusAsync(string userId)
    {
        var profile = await _context.GitHubProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (profile == null)
        {
            return new GitHubConnectionDto { IsConnected = false };
        }

        return new GitHubConnectionDto
        {
            IsConnected = true,
            GitHubUsername = profile.GitHubUsername,
            ConnectedAt = profile.ConnectedAt,
            LastSyncedAt = profile.LastSyncedAt
        };
    }

    public async Task<bool> DisconnectAsync(string userId)
    {
        var profile = await _context.GitHubProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
            return false;

        profile.IsActive = false;
        profile.AccessToken = null;

        await _context.SaveChangesAsync();

        _logger.LogInformation("GitHub disconnected for user {UserId}", userId);

        return true;
    }

    public async Task<GitHubStatsDto> GetGitHubStatsAsync(string userId)
    {
        var profile = await _context.GitHubProfiles
            .Include(p => p.Activities)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (profile == null)
            throw new InvalidOperationException("GitHub not connected");

        var activities = profile.Activities.ToList();

        var stats = new GitHubStatsDto
        {
            TotalRepositories = activities.Count,
            TotalCommits = activities.Sum(a => a.CommitCount),
            LastSyncedAt = profile.LastSyncedAt
        };

        // Aggregate language breakdown
        var languageBreakdown = new Dictionary<string, int>();

        foreach (var activity in activities)
        {
            if (!string.IsNullOrEmpty(activity.LanguageBreakdown))
            {
                try
                {
                    var langs = JsonSerializer.Deserialize<Dictionary<string, int>>(activity.LanguageBreakdown);
                    if (langs != null)
                    {
                        foreach (var kvp in langs)
                        {
                            if (languageBreakdown.ContainsKey(kvp.Key))
                                languageBreakdown[kvp.Key] += kvp.Value;
                            else
                                languageBreakdown[kvp.Key] = kvp.Value;
                        }
                    }
                }
                catch { }
            }
        }

        stats.LanguageBreakdown = languageBreakdown;

        return stats;
    }

    public async Task SyncGitHubDataAsync(string userId)
    {
        var profile = await _context.GitHubProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (profile == null || string.IsNullOrEmpty(profile.AccessToken))
            throw new InvalidOperationException("GitHub not connected");

        try
        {
            var client = new GitHubClient(new ProductHeaderValue("SkillSync"))
            {
                Credentials = new Credentials(profile.AccessToken)
            };

            // Fetch user's repositories
            var repos = await client.Repository.GetAllForCurrent();

            // Clear old activities
            var oldActivities = await _context.GitHubActivities
                .Where(a => a.GitHubProfileId == profile.Id)
                .ToListAsync();

            _context.GitHubActivities.RemoveRange(oldActivities);

            // Add new activities
            foreach (var repo in repos.Where(r => !r.Fork).Take(20)) // Limit to 20 repos
            {
                try
                {
                    // Get language stats
                    var languages = await client.Repository.GetAllLanguages(repo.Id);
                    var languageBreakdown = languages.ToDictionary(l => l.Name, l => (int)l.NumberOfBytes);

                    // Get commit count (approximate)
                    var commits = await client.Repository.Commit.GetAll(repo.Id, new CommitRequest { Until = DateTimeOffset.Now });

                    var activity = new Data.Entities.GitHubActivity
                    {
                        GitHubProfileId = profile.Id,
                        RepositoryName = repo.Name,
                        RepositoryUrl = repo.HtmlUrl,
                        PrimaryLanguage = repo.Language ?? "Unknown",
                        CommitCount = commits.Count,
                        LastCommitDate = repo.UpdatedAt.DateTime,
                        RecordedAt = DateTime.UtcNow,
                        LanguageBreakdown = JsonSerializer.Serialize(languageBreakdown)
                    };

                    _context.GitHubActivities.Add(activity);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error fetching data for repo {RepoName}", repo.Name);
                }
            }

            profile.LastSyncedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("GitHub data synced for user {UserId}", userId);

            // Auto-map languages to skills
            await AutoMapLanguagesToSkillsAsync(userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing GitHub data");
            throw;
        }
    }

    public async Task AutoMapLanguagesToSkillsAsync(string userId)
    {
        var profile = await _context.GitHubProfiles
            .Include(p => p.Activities)
            .FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

        if (profile == null)
            return;

        // Get all languages from GitHub activities
        var languages = profile.Activities
            .Select(a => a.PrimaryLanguage)
            .Where(l => !string.IsNullOrEmpty(l))
            .Distinct()
            .ToList();

        // Get programming languages category
        var progCategory = await _context.SkillCategories
            .FirstOrDefaultAsync(c => c.Name == "Programming Languages");

        if (progCategory == null)
            return;

        // Get existing user skills
        var existingSkills = await _context.Skills
            .Where(s => s.UserId == userId)
            .ToListAsync();

        var createdSkills = new List<string>();

        foreach (var language in languages)
        {
            // Check if skill already exists
            var exists = existingSkills.Any(s =>
                s.Name.Equals(language, StringComparison.OrdinalIgnoreCase));

            if (!exists)
            {
                // Auto-create skill
                var skill = new Skill
                {
                    UserId = userId,
                    Name = language,
                    Description = $"Automatically detected from GitHub repositories",
                    ProficiencyLevel = 2, // Start at beginner-intermediate
                    TargetLevel = 4,
                    CategoryId = progCategory.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                _context.Skills.Add(skill);
                createdSkills.Add(language);
            }
        }

        if (createdSkills.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("Auto-created {Count} skills for user {UserId}: {Skills}",
                createdSkills.Count, userId, string.Join(", ", createdSkills));
        }
    }

    private async Task<string?> ExchangeCodeForTokenAsync(string code)
    {
        try
        {
            var requestData = new Dictionary<string, string>
            {
                { "client_id", _settings.ClientId },
                { "client_secret", _settings.ClientSecret },
                { "code", code }
            };

            var response = await _httpClient.PostAsync(
                "https://github.com/login/oauth/access_token",
                new FormUrlEncodedContent(requestData));

            var content = await response.Content.ReadAsStringAsync();

            // Parse response (format: access_token=xxx&scope=xxx&token_type=bearer)
            var parameters = content.Split('&')
                .Select(p => p.Split('='))
                .ToDictionary(p => p[0], p => p.Length > 1 ? p[1] : "");

            if (parameters.TryGetValue("access_token", out var token))
                return token;

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exchanging code for token");
            return null;
        }
    }
}