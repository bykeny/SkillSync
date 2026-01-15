namespace SkillSync.Api.Services.AI;

public interface IRateLimitService
{
    /// <summary>
    /// Waits if necessary to respect rate limits before making an API call
    /// </summary>
    /// <returns>Task that completes when the call can be made</returns>
    Task WaitForRateLimitAsync();
    
    /// <summary>
    /// Records that an API call was made
    /// </summary>
    void RecordApiCall();
    
    /// <summary>
    /// Gets the current status of rate limits
    /// </summary>
    /// <returns>A tuple containing (requests in last minute, requests today)</returns>
    (int RequestsPerMinute, int RequestsToday) GetRateLimitStatus();
}
