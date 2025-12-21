namespace SkillSync.Api.DTOs.GitHub;

public class GitHubConnectionDto
{
    public bool IsConnected { get; set; }
    public string? GitHubUsername { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}