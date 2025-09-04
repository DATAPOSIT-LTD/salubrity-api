using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Notifications;
using Salubrity.Domain.Entities.Notifications;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Notifications
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification, CancellationToken ct = default)
        {
            await _context.Notifications.AddAsync(notification, ct);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Notifications
                .Include(n => n.Recipients)
                .FirstOrDefaultAsync(n => n.Id == id, ct);
        }

        public async Task<IEnumerable<Notification>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Notifications
                .Include(n => n.Recipients)
                .ToListAsync(ct);
        }

        public async Task UpdateAsync(Notification notification, CancellationToken ct = default)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync(ct);
        }
    }
}