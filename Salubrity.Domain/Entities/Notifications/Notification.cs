using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.Notifications
{
    public class Notification : BaseAuditableEntity
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public ICollection<NotificationRecipient> Recipients { get; set; } = new List<NotificationRecipient>();
    }
}