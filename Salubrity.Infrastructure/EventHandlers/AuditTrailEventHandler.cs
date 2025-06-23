using MediatR;
using Salubrity.Domain.Events.Audit;
using Salubrity.Domain.Entities.Audit;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.EventHandlers
{
    public class AuditTrailEventHandler : INotificationHandler<AuditTrailEvent>
    {
        private readonly AppDbContext _db;

        public AuditTrailEventHandler(AppDbContext db)
        {
            _db = db;
        }

        public async Task Handle(AuditTrailEvent notification, CancellationToken cancellationToken)
        {
            var log = new AuditLog
            {
                EntityId = notification.EntityId,
                EntityType = notification.EntityType,
                Action = notification.Action,
                UserId = notification.UserId,
                OccurredAt = notification.OccurredAt
            };

            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}

