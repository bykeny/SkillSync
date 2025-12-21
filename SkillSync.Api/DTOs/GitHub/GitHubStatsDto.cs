namespace SkillSync.Api.DTOs.GitHub;

public class GitHubStatsDto
{
    public int TotalRepositories { get; set; }
    public int TotalCommits { get; set; }
    public Dictionary<string, int> LanguageBreakdown { get; set; } = new();
    public List<GitHubRepoDto> TopRepositories { get; set; } = new();
    public DateTime? LastSyncedAt { get; set; }
}