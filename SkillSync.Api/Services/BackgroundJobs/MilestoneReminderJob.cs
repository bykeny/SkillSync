using Microsoft.EntityFrameworkCore;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;
using SkillSync.Api.Services.Notifications;

namespace SkillSync.Api.Services.BackgroundJobs;

public class MilestoneReminderJob
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<MilestoneReminderJob> _logger;

    public MilestoneReminderJob(
        ApplicationDbContext context,
        IEmailService emailService,
        INotificationService notificationService,
        ILogger<MilestoneReminderJob> logger)
    {
        _context = context;
        _emailService = emailService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        _logger.LogInformation("Starting milestone reminder job");

        var threeDaysFromNow = DateTime.UtcNow.AddDays(3);

        var upcomingMilestones = await _context.Milestones
            .Include(m => m.Skill)
                .ThenInclude(s => s.User)
            .Where(m => !m.IsCompleted
                && m.TargetDate <= threeDaysFromNow
                && m.TargetDate >= DateTime.UtcNow)
            .ToListAsync();

        foreach (var milestone in upcomingMilestones)
        {
            try
            {
                // Send email reminder
                await _emailService.SendMilestoneReminderAsync(milestone.Skill.UserId, milestone.Id);

                // Create in-app notification
                var daysRemaining = (milestone.TargetDate - DateTime.UtcNow).Days;
                await _notificationService.CreateNotificationAsync(
                    milestone.Skill.UserId,
                    "Milestone Reminder",
                    $"Your milestone '{milestone.Title}' is due in {daysRemaining} days!",
                    NotificationType.MilestoneReminder,
                    $"/skills"
                );

                _logger.LogInformation("Reminder sent for milestone {MilestoneId}", milestone.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder for milestone {MilestoneId}", milestone.Id);
            }
        }

        _logger.LogInformation("Milestone reminder job completed. Processed {Count} milestones", upcomingMilestones.Count);
    }
}