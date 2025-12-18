namespace SkillSync.Api.Data.Entities;
public class GitHubProfile
{
    public int Id { get; set; }
    public string GitHubUsername { get; set; } = string.Empty;
    public string? AccessToken { get; set; } // Encrypted in production
    public DateTime? LastSyncedAt { get; set; }
    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Foreign key
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;

    // Navigation properties
    public virtual ICollection<GitHubActivity> Activities { get; set; } = new List<GitHubActivity>();
}