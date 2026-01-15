using System.Collections.Concurrent;

namespace SkillSync.Api.Services.AI;

/// <summary>
/// Rate limiting service to prevent exceeding Google Gemini API free tier limits
/// Free tier limits for gemini-1.5-flash: 15 RPM (requests per minute) and 1500 RPD (requests per day)
/// </summary>
public class RateLimitService : IRateLimitService
{
    private const int MaxRequestsPerMinute = 15;
    private const int MaxRequestsPerDay = 1500;
    private const int MinMillisecondsBetweenRequests = 4000; // ~4 seconds to stay safe (15 requests / 60 seconds)
    
    private readonly ConcurrentQueue<DateTime> _requestTimestamps = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private DateTime _lastRequestTime = DateTime.MinValue;
    private readonly ILogger<RateLimitService> _logger;

    public RateLimitService(ILogger<RateLimitService> logger)
    {
        _logger = logger;
    }

    public async Task WaitForRateLimitAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            CleanOldTimestamps();
            
            var now = DateTime.UtcNow;
            var requestsInLastMinute = _requestTimestamps.Count(t => (now - t).TotalMinutes < 1);
            var requestsToday = _requestTimestamps.Count(t => now.Date == t.Date);

            // Check daily limit
            if (requestsToday >= MaxRequestsPerDay)
            {
                var timeUntilMidnight = DateTime.UtcNow.Date.AddDays(1) - DateTime.UtcNow;
                _logger.LogWarning("Daily API request limit reached ({MaxRequests} requests). No more requests until midnight (in {Hours:F1} hours)", 
                    MaxRequestsPerDay, timeUntilMidnight.TotalHours);
                throw new InvalidOperationException($"Daily API request limit of {MaxRequestsPerDay} requests has been reached. Please try again tomorrow.");
            }

            // Check per-minute limit
            if (requestsInLastMinute >= MaxRequestsPerMinute)
            {
                var oldestInMinute = _requestTimestamps
                    .Where(t => (now - t).TotalMinutes < 1)
                    .OrderBy(t => t)
                    .First();
                var waitTime = TimeSpan.FromMinutes(1) - (now - oldestInMinute) + TimeSpan.FromMilliseconds(100);
                
                _logger.LogInformation("Rate limit approaching. Waiting {Seconds:F1} seconds before next request", waitTime.TotalSeconds);
                await Task.Delay(waitTime);
            }

            // Ensure minimum time between requests
            var timeSinceLastRequest = now - _lastRequestTime;
            if (timeSinceLastRequest < TimeSpan.FromMilliseconds(MinMillisecondsBetweenRequests))
            {
                var additionalWait = TimeSpan.FromMilliseconds(MinMillisecondsBetweenRequests) - timeSinceLastRequest;
                _logger.LogDebug("Enforcing minimum delay between requests: {Milliseconds}ms", additionalWait.TotalMilliseconds);
                await Task.Delay(additionalWait);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void RecordApiCall()
    {
        var now = DateTime.UtcNow;
        _requestTimestamps.Enqueue(now);
        _lastRequestTime = now;
        
        var (rpm, rpd) = GetRateLimitStatus();
        _logger.LogInformation("API call recorded. Current usage: {RPM}/{MaxRPM} RPM, {RPD}/{MaxRPD} RPD", 
            rpm, MaxRequestsPerMinute, rpd, MaxRequestsPerDay);
    }

    public (int RequestsPerMinute, int RequestsToday) GetRateLimitStatus()
    {
        CleanOldTimestamps();
        var now = DateTime.UtcNow;
        var requestsInLastMinute = _requestTimestamps.Count(t => (now - t).TotalMinutes < 1);
        var requestsToday = _requestTimestamps.Count(t => now.Date == t.Date);
        return (requestsInLastMinute, requestsToday);
    }

    private void CleanOldTimestamps()
    {
        var cutoffTime = DateTime.UtcNow.AddDays(-1);
        while (_requestTimestamps.TryPeek(out var oldest) && oldest < cutoffTime)
        {
            _requestTimestamps.TryDequeue(out _);
        }
    }
}
