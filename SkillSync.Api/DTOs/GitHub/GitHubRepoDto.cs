namespace SkillSync.Api.DTOs.GitHub;

public class GitHubRepoDto
{
    public string Name { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string HtmlUrl { get; set; } = string.Empty;
    public string? Language { get; set; }
    public int StargazersCount { get; set; }
    public int ForksCount { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Dictionary<string, long> Languages { get; set; } = new();
}