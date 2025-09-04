using Salubrity.Application.DTOs.Notifications;

namespace Salubrity.Application.Interfaces.Services.Notifications
{
    public interface INotificationService
    {
        Task TriggerNotificationAsync(string title, string message, string type, Guid entityId, string entityType, CancellationToken ct = default);
        Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default);
        Task<bool> MarkNotificationAsReadAsync(Guid notificationId, CancellationToken ct = default);
    }
}