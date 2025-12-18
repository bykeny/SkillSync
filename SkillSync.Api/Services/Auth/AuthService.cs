using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.DTOs.Auth;

namespace SkillSync.Api.Services.Auth;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtSettings)
    {
        _userManager = userManager;
        _context = context;
        _tokenService = tokenService;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task<(bool Success, string Message, TokenResponseDto? Token)> RegisterAsync(RegisterDto dto)
    {
        // Check if user exists
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return (false, "User with this email already exists", null);

        // Check password confirmation
        if (dto.Password != dto.ConfirmPassword)
            return (false, "Passwords do not match", null);

        // Create user
        var user = new ApplicationUser
        {
            Email = dto.Email,
            UserName = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, errors, null);
        }

        // Generate tokens
        var tokenResponse = await GenerateTokenResponseAsync(user);
        return (true, "User registered successfully", tokenResponse);
    }

    public async Task<(bool Success, string Message, TokenResponseDto? Token)> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return (false, "Invalid email or password", null);

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            return (false, "Invalid email or password", null);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        var tokenResponse = await GenerateTokenResponseAsync(user);
        return (true, "Login successful", tokenResponse);
    }

    public async Task<(bool Success, string Message, TokenResponseDto? Token)> RefreshTokenAsync(string refreshToken)
    {
        var validToken = await _tokenService.ValidateRefreshTokenAsync(refreshToken);
        if (validToken == null)
            return (false, "Invalid or expired refresh token", null);

        // Revoke old token
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, "Replaced by new token");

        // Generate new tokens
        var tokenResponse = await GenerateTokenResponseAsync(validToken.User);

        // Store replacement info
        validToken.ReplacedByToken = tokenResponse.RefreshToken;
        await _context.SaveChangesAsync();

        return (true, "Token refreshed successfully", tokenResponse);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        await _tokenService.RevokeRefreshTokenAsync(refreshToken, "Revoked by user");
        return true;
    }

    public async Task<UserProfileDto?> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return null;

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            GitHubUsername = user.GitHubUsername,
            ProfileImageUrl = user.ProfileImageUrl,
            CreatedAt = user.CreatedAt
        };
    }

    private async Task<TokenResponseDto> GenerateTokenResponseAsync(ApplicationUser user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        var expiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

        // Store refresh token in database
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshTokenEntity);
        await _context.SaveChangesAsync();

        return new TokenResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }
}