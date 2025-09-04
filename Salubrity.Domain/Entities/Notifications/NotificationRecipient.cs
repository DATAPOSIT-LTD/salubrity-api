namespace Salubrity.Domain.Entities.Notifications
{
    public class NotificationRecipient
    {
        public Guid Id { get; set; }
        public Guid NotificationId { get; set; }
        public Guid RecipientId { get; set; }
        public string RecipientType { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        public Notification Notification { get; set; }
    }
}