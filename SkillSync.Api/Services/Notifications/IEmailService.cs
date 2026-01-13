namespace SkillSync.Api.Services.Notifications;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
    Task SendWeeklySummaryAsync(string userId);
    Task SendMilestoneReminderAsync(string userId, int milestoneId);
}