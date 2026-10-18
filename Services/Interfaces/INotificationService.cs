using AbaDeskBack.DTOs.Notifications;
using AbaDeskBack.Enums;

namespace AbaDeskBack.Services.Interfaces;

public interface INotificationService
{
    Task<List<NotificationResponse>> GetNotificationsAsync(Guid userId);
    Task CreateNotificationAsync(Guid userId, Guid? ticketId, string title, string message, NotificationType type);
    Task DeleteNotificationAsync(Guid notificationId, Guid userId);
    Task ClearAllNotificationsAsync(Guid userId);
}
