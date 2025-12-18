using SkillSync.Api.Data.Entities;

namespace SkillSync.Api.Services.Auth;

public interface ITokenService
{
    string GenerateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
    Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token, string reason);
}