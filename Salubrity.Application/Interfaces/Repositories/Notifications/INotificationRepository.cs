using Salubrity.Domain.Entities.Notifications;

namespace Salubrity.Application.Interfaces.Repositories.Notifications
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification, CancellationToken ct = default);
        Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IEnumerable<Notification>> GetAllAsync(CancellationToken ct = default);
        Task UpdateAsync(Notification notification, CancellationToken ct = default);
        Task MarkAllAsReadForUserAsync(Guid userId, CancellationToken ct = default);
        Task ClearAllForUserAsync(Guid userId, CancellationToken ct = default);
    }
}