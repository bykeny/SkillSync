using Microsoft.EntityFrameworkCore;
using SkillSync.Api.Data;
using SkillSync.Api.Data.Entities;

namespace SkillSync.Api.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateNotificationAsync(
        string userId,
        string title,
        string message,
        NotificationType type,
        string? actionUrl = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            ActionUrl = actionUrl,
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Notification created for user {UserId}: {Title}", userId, title);
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        var query = _context.Notifications.Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification != null && !notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }
}