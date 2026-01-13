using Microsoft.EntityFrameworkCore;
using SkillSync.Api.Data;
using SkillSync.Api.Services.Notifications;

namespace SkillSync.Api.Services.BackgroundJobs;

public class WeeklySummaryJob
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<WeeklySummaryJob> _logger;

    public WeeklySummaryJob(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<WeeklySummaryJob> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting weekly summary job");

        var users = await _context.Users
            .Where(u => !string.IsNullOrEmpty(u.Email))
            .ToListAsync();

        foreach (var user in users)
        {
            try
            {
                await _emailService.SendWeeklySummaryAsync(user.Id);
                _logger.LogInformation("Weekly summary sent to {Email}", user.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send weekly summary to {Email}", user.Email);
            }
        }

        _logger.LogInformation("Weekly summary job completed. Sent to {Count} users", users.Count);
    }
}