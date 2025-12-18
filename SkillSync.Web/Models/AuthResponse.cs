namespace SkillSync.Web.Models;

public class AuthResponse
{
    public string Message { get; set; } = string.Empty;
    public TokenResponse? Token { get; set; }
}