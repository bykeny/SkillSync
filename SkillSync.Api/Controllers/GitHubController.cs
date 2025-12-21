using System.Collections.Concurrent;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSync.Api.Services.GitHub;

namespace SkillSync.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GitHubController : ControllerBase
{
    private readonly IGitHubService _gitHubService;
    private readonly ILogger<GitHubController> _logger;

    // In-memory store for OAuth state -> userId mapping
    // In production, use a distributed cache like Redis
    private static readonly ConcurrentDictionary<string, (string UserId, DateTime ExpiresAt)> _stateStore = new();

    public GitHubController(IGitHubService gitHubService, ILogger<GitHubController> logger)
    {
        _gitHubService = gitHubService;
        _logger = logger;
    }

    [HttpGet("connect")]
    public async Task<IActionResult> Connect()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Generate state and store mapping to userId
        var state = Guid.NewGuid().ToString();
        _stateStore[state] = (userId, DateTime.UtcNow.AddMinutes(10));

        // Clean up expired states
        CleanupExpiredStates();

        var authUrl = await _gitHubService.GetAuthorizationUrlAsync(state);

        return Ok(new { authUrl, state });
    }

    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
    {
        // Verify state and get userId
        if (!_stateStore.TryRemove(state, out var stateData))
        {
            _logger.LogWarning("Invalid or expired OAuth state: {State}", state);
            return Content(@"
                <html>
                <body>
                    <h2>Error</h2>
                    <p>Invalid or expired authorization. Please try again.</p>
                </body>
                </html>
            ", "text/html");
        }

        if (stateData.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Expired OAuth state: {State}", state);
            return Content(@"
                <html>
                <body>
                    <h2>Error</h2>
                    <p>Authorization expired. Please try again.</p>
                </body>
                </html>
            ", "text/html");
        }

        // Complete the connection directly
        var success = await _gitHubService.HandleCallbackAsync(code, stateData.UserId);

        if (!success)
        {
            return Content(@"
                <html>
                <body>
                    <h2>Error</h2>
                    <p>Failed to connect GitHub account. Please try again.</p>
                </body>
                </html>
            ", "text/html");
        }

        return Content(@"
            <html>
            <body>
                <h2>GitHub Connected!</h2>
                <p>You can close this window now.</p>
                <script>
                    if (window.opener) {
                        window.opener.postMessage({ type: 'github-callback', success: true }, '*');
                        setTimeout(() => window.close(), 2000);
                    }
                </script>
            </body>
            </html>
        ", "text/html");
    }

    private static void CleanupExpiredStates()
    {
        var expiredStates = _stateStore
            .Where(kvp => kvp.Value.ExpiresAt < DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredStates)
        {
            _stateStore.TryRemove(key, out _);
        }
    }

    [HttpPost("complete-connection")]
    public async Task<IActionResult> CompleteConnection([FromBody] CompleteConnectionRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _gitHubService.HandleCallbackAsync(request.Code, userId);

        if (!success)
            return BadRequest(new { message = "Failed to connect GitHub account" });

        return Ok(new { message = "GitHub connected successfully" });
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var status = await _gitHubService.GetConnectionStatusAsync(userId);
        return Ok(status);
    }

    [HttpPost("disconnect")]
    public async Task<IActionResult> Disconnect()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var success = await _gitHubService.DisconnectAsync(userId);

        if (!success)
            return NotFound(new { message = "GitHub not connected" });

        return Ok(new { message = "GitHub disconnected successfully" });
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            var stats = await _gitHubService.GetGitHubStatsAsync(userId);
            return Ok(stats);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        try
        {
            await _gitHubService.SyncGitHubDataAsync(userId);
            return Ok(new { message = "GitHub data synced successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CompleteConnectionRequest
{
    public string Code { get; set; } = string.Empty;
}