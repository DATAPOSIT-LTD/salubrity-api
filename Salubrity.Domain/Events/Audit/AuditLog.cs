using Salubrity.Domain.Common;
using System;

namespace Salubrity.Domain.Entities.Audit
{
    public class AuditLog : BaseAuditableEntity
    {
        public Guid EntityId { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
