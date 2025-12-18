using Microsoft.AspNetCore.Identity;
using System.Diagnostics;

namespace SkillSync.Api.Data.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
    public string? GitHubUsername { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Navigation properties
    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
    public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public virtual GitHubProfile? GitHubProfile { get; set; }
    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}