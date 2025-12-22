namespace SkillSync.Web.Models;

public class GitHubStats
{
    public int TotalRepositories { get; set; }
    public int TotalCommits { get; set; }
    public Dictionary<string, int> LanguageBreakdown { get; set; } = new();
    public DateTime? LastSyncedAt { get; set; }
}