namespace SkillSync.Api.Data.Entities;
public class GitHubActivity
{
    public int Id { get; set; }
    public string RepositoryName { get; set; } = string.Empty;
    public string? RepositoryUrl { get; set; }
    public string PrimaryLanguage { get; set; } = string.Empty;
    public int CommitCount { get; set; }
    public DateTime LastCommitDate { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string? LanguageBreakdown { get; set; } // JSON string: {"C#": 45, "JavaScript": 30, ...}

    // Foreign key
    public int GitHubProfileId { get; set; }
    public virtual GitHubProfile GitHubProfile { get; set; } = null!;
}