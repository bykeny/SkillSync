using SkillSync.Api.Data.Entities;

namespace SkillSync.Api.Services.Notifications;

public interface INotificationService
{
    Task CreateNotificationAsync(string userId, string title, string message, NotificationType type, string? actionUrl = null);
    Task<List<Data.Entities.Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task MarkAsReadAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
}