namespace SkillSync.Web.Models;

public class GitHubConnection
{
    public bool IsConnected { get; set; }
    public string? GitHubUsername { get; set; }
    public DateTime? ConnectedAt { get; set; }
    public DateTime? LastSyncedAt { get; set; }
}