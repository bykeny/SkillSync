namespace SkillSync.Web.Models;

public class UserProfile
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? GitHubUsername { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Initials => $"{FirstName?[0]}{LastName?[0]}".ToUpper();
}