using MediatR;
using System;

namespace Salubrity.Domain.Events.Audit
{
    public class AuditTrailEvent : INotification
    {
        public AuditTrailEvent(Guid entityId, string entityType, string action, Guid? userId)
        {
            EntityId = entityId;
            EntityType = entityType;
            Action = action;
            UserId = userId;
            OccurredAt = DateTime.UtcNow;
        }

        public Guid EntityId { get; }
        public string EntityType { get; }
        public string Action { get; } // e.g. "Created", "Updated", "Deleted"
        public Guid? UserId { get; }
        public DateTime OccurredAt { get; }
    }
}
