using SkillSync.Api.DTOs.Auth;

namespace SkillSync.Api.Services.Auth;

public interface IAuthService
{
    Task<(bool Success, string Message, TokenResponseDto? Token)> RegisterAsync(RegisterDto dto);
    Task<(bool Success, string Message, TokenResponseDto? Token)> LoginAsync(LoginDto dto);
    Task<(bool Success, string Message, TokenResponseDto? Token)> RefreshTokenAsync(string refreshToken);
    Task<bool> RevokeTokenAsync(string refreshToken);
    Task<UserProfileDto?> GetUserProfileAsync(string userId);
}