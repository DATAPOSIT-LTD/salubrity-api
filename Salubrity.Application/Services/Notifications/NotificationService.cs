using Salubrity.Application.DTOs.Notifications;
using Salubrity.Application.Interfaces.Repositories.Notifications;
using Salubrity.Application.Interfaces.Repositories.Users;
using Salubrity.Application.Interfaces.Services.Notifications;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Notifications;

namespace Salubrity.Application.Services.Notifications
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        public NotificationService(INotificationRepository notificationRepository, IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _userRepository = userRepository;
        }

        public async Task TriggerNotificationAsync(string title, string message, string type, Guid entityId, string entityType, CancellationToken ct = default)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                Title = title,
                Message = message,
                Type = type,
                CreatedAt = DateTime.Now,
                Recipients = new List<NotificationRecipient>()
            };

            IEnumerable<User> users = await GetUsersByEntityTypeAsync(entityId, entityType, ct);

            foreach (var user in users)
            {
                notification.Recipients.Add(new NotificationRecipient
                {
                    Id = Guid.NewGuid(),
                    NotificationId = notification.Id,
                    RecipientId = user.Id,
                    RecipientType = entityType,
                    IsRead = false
                });
            }

            await _notificationRepository.AddAsync(notification, ct);
        }

        private async Task<IEnumerable<User>> GetUsersByEntityTypeAsync(Guid entityId, string entityType, CancellationToken ct)
        {
            var users = await _userRepository.GetAllAsync(ct);

            return entityType switch
            {
                "Organization" => users.Where(u => u.OrganizationId == entityId),
                "Camp" => users.Where(u => u.RelatedEntityId == entityId && u.RelatedEntityType == "HealthCampParticipant"),
                "Role" => users.Where(u => u.UserRoles.Any(r => r.RoleId == entityId)),
                "Subcontractor" => users.Where(u => u.RelatedEntityId == entityId && u.RelatedEntityType == "Subcontractor"),
                //"Service" => users.Where(u => /* TODO: Add your logic for service-related users */),
                "Employee" => users.Where(u => u.Id == entityId /* or your logic for employee-related users */),
                "User" => users.Where(u => u.Id == entityId),
                "Onboarding" => users.Where(u => u.Id == entityId),
                //"CampQueue" => users.Where(u => /* TODO: Add your logic for camp queue event users */),
                _ => throw new ArgumentException("Invalid entity type", nameof(entityType))
            };
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(Guid userId, CancellationToken ct = default)
        {
            var notifications = await _notificationRepository.GetAllAsync(ct);
            return notifications
                .Where(n => n.Recipients.Any(r => r.RecipientId == userId))
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Message = n.Message,
                    Type = n.Type,
                    CreatedAt = n.CreatedAt,
                    IsRead = n.Recipients.First(r => r.RecipientId == userId).IsRead
                })
                .ToList();
        }

        public async Task<bool> MarkNotificationAsReadAsync(Guid notificationId, CancellationToken ct = default)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId, ct);
            if (notification == null) return false;

            foreach (var recipient in notification.Recipients)
            {
                recipient.IsRead = true;
                recipient.ReadAt = DateTime.Now;
            }

            await _notificationRepository.UpdateAsync(notification, ct);
            return true;
        }
    }
}