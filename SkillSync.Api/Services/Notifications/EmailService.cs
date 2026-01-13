using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SkillSync.Api.Data;

namespace SkillSync.Api.Services.Notifications;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailSettings> settings,
        ApplicationDbContext context,
        ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _context = context;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, _settings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);

            _logger.LogInformation("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            throw;
        }
    }

    public async Task SendWeeklySummaryAsync(string userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.Email))
            return;

        var weekStart = DateTime.UtcNow.AddDays(-7);

        // Get weekly stats
        var activities = await _context.Activities
            .Where(a => a.UserId == userId && a.CreatedAt >= weekStart)
            .ToListAsync();

        var skills = await _context.Skills
            .Where(s => s.UserId == userId && s.IsActive)
            .ToListAsync();

        var completedCount = activities.Count(a => a.Status == Data.Entities.ActivityStatus.Completed);
        var totalHours = activities.Sum(a => a.DurationMinutes) / 60.0;

        var subject = $"📊 Your Weekly SkillSync Summary";
        var body = BuildWeeklySummaryHtml(user.FirstName ?? "there", completedCount, totalHours, skills.Count);

        await SendEmailAsync(user.Email, subject, body);
    }

    public async Task SendMilestoneReminderAsync(string userId, int milestoneId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null || string.IsNullOrEmpty(user.Email))
            return;

        var milestone = await _context.Milestones
            .Include(m => m.Skill)
            .FirstOrDefaultAsync(m => m.Id == milestoneId);

        if (milestone == null || milestone.IsCompleted)
            return;

        var daysRemaining = (milestone.TargetDate - DateTime.UtcNow).Days;

        var subject = $"⏰ Milestone Reminder: {milestone.Title}";
        var body = BuildMilestoneReminderHtml(
            user.FirstName ?? "there",
            milestone.Title,
            milestone.Skill.Name,
            milestone.TargetDate,
            daysRemaining);

        await SendEmailAsync(user.Email, subject, body);
    }

    private string BuildWeeklySummaryHtml(string name, int completedActivities, double totalHours, int skillCount)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; }}
        .stat-card {{ background: white; padding: 20px; margin: 10px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .stat-number {{ font-size: 32px; font-weight: bold; color: #667eea; }}
        .footer {{ background: #374151; color: white; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📊 Weekly Summary</h1>
            <p>Your learning progress this week</p>
        </div>
        <div class='content'>
            <p>Hi {name},</p>
            <p>Here's a summary of your learning journey this week:</p>
            
            <div class='stat-card'>
                <div class='stat-number'>{completedActivities}</div>
                <p>Activities Completed</p>
            </div>
            
            <div class='stat-card'>
                <div class='stat-number'>{totalHours:F1}h</div>
                <p>Total Learning Time</p>
            </div>
            
            <div class='stat-card'>
                <div class='stat-number'>{skillCount}</div>
                <p>Active Skills Tracked</p>
            </div>
            
            <p>Keep up the great work! 💪</p>
            
            <a href='https://localhost:7074/analytics' class='button'>View Detailed Analytics</a>
        </div>
        <div class='footer'>
            <p>SkillSync - Track Your Learning Journey</p>
            <p style='font-size: 12px; color: #9ca3af;'>You're receiving this because you're using SkillSync</p>
        </div>
    </div>
</body>
</html>";
    }

    private string BuildMilestoneReminderHtml(string name, string milestoneTitle, string skillName, DateTime targetDate, int daysRemaining)
    {
        var urgency = daysRemaining <= 3 ? "urgent" : "upcoming";
        var color = daysRemaining <= 3 ? "#ef4444" : "#f59e0b";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: {color}; color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9fafb; padding: 30px; }}
        .reminder-box {{ background: white; border-left: 4px solid {color}; padding: 20px; margin: 20px 0; border-radius: 5px; }}
        .button {{ display: inline-block; padding: 12px 30px; background: {color}; color: white; text-decoration: none; border-radius: 5px; margin: 10px 0; }}
        .footer {{ background: #374151; color: white; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⏰ Milestone Reminder</h1>
        </div>
        <div class='content'>
            <p>Hi {name},</p>
            <p>This is a {urgency} reminder about your milestone:</p>
            
            <div class='reminder-box'>
                <h2>{milestoneTitle}</h2>
                <p><strong>Skill:</strong> {skillName}</p>
                <p><strong>Target Date:</strong> {targetDate:MMMM dd, yyyy}</p>
                <p><strong>Days Remaining:</strong> {daysRemaining} {(daysRemaining == 1 ? "day" : "days")}</p>
            </div>
            
            {(daysRemaining <= 3 ? "<p><strong>⚠️ This milestone is due very soon!</strong></p>" : "<p>Keep making progress toward your goal!</p>")}
            
            <a href='https://localhost:7074/skills' class='button'>View Milestone</a>
        </div>
        <div class='footer'>
            <p>SkillSync - Track Your Learning Journey</p>
        </div>
    </div>
</body>
</html>";
    }
}