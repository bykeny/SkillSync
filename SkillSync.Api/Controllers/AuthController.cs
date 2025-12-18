using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSync.Api.DTOs.Auth;
using SkillSync.Api.Services.Auth;

namespace SkillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        _logger.LogInformation("Registration attempt for email: {Email}", dto.Email);

        var (success, message, token) = await _authService.RegisterAsync(dto);

        if (!success)
        {
            _logger.LogWarning("Registration failed for {Email}: {Message}", dto.Email, message);
            return BadRequest(new { message });
        }

        _logger.LogInformation("User registered successfully: {Email}", dto.Email);
        return Ok(new { message, token });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        _logger.LogInformation("Login attempt for email: {Email}", dto.Email);

        var (success, message, token) = await _authService.LoginAsync(dto);

        if (!success)
        {
            _logger.LogWarning("Login failed for {Email}: {Message}", dto.Email, message);
            return Unauthorized(new { message });
        }

        _logger.LogInformation("User logged in successfully: {Email}", dto.Email);
        return Ok(new { message, token });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        _logger.LogInformation("Token refresh attempt");

        var (success, message, token) = await _authService.RefreshTokenAsync(dto.RefreshToken);

        if (!success)
        {
            _logger.LogWarning("Token refresh failed: {Message}", message);
            return Unauthorized(new { message });
        }

        _logger.LogInformation("Token refreshed successfully");
        return Ok(new { message, token });
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto dto)
    {
        _logger.LogInformation("Token revocation attempt");

        var success = await _authService.RevokeTokenAsync(dto.RefreshToken);

        if (!success)
        {
            _logger.LogWarning("Token revocation failed");
            return BadRequest(new { message = "Failed to revoke token" });
        }

        _logger.LogInformation("Token revoked successfully");
        return Ok(new { message = "Token revoked successfully" });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { message = "Invalid token" });

        var profile = await _authService.GetUserProfileAsync(userId);
        if (profile == null)
            return NotFound(new { message = "User not found" });

        return Ok(profile);
    }

    [HttpGet("test-auth")]
    [Authorize]
    public IActionResult TestAuth()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = User.FindFirstValue(ClaimTypes.Email);

        return Ok(new
        {
            message = "You are authenticated!",
            userId,
            email
        });
    }
}