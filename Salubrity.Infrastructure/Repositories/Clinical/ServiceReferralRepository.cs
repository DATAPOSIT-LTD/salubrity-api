// File: Infrastructure/Repositories/Clinical/ServiceReferralRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Clinical;
using Salubrity.Domain.Entities.Clinical;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Clinical
{
    public class ServiceReferralRepository : IServiceReferralRepository
    {
        private readonly AppDbContext _context;

        public ServiceReferralRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceReferral?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var r = await _context.ServiceReferrals
                .Include(x => x.Urgency)
                .Include(x => x.FollowUpSchedule)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
            if (r != null) await EnrichAsync(new List<ServiceReferral> { r }, ct);
            return r;
        }

        public async Task<IReadOnlyList<ServiceReferral>> GetByParticipantAndCampAsync(Guid participantId, Guid healthCampId, CancellationToken ct = default)
        {
            var list = await _context.ServiceReferrals.Include(r => r.Urgency)
                .Include(r => r.FollowUpSchedule)
                .Where(r => r.ParticipantId == participantId && r.HealthCampId == healthCampId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
            await EnrichAsync(list, ct);
            return list;
        }

        public async Task<IReadOnlyList<ServiceReferral>> GetByCampAsync(Guid healthCampId, CancellationToken ct = default)
        {
            var list = await _context.ServiceReferrals.Include(r => r.Urgency)
                .Include(r => r.FollowUpSchedule)
                .Where(r => r.HealthCampId == healthCampId && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);
            await EnrichAsync(list, ct);
            return list;
        }

        public async Task<ServiceReferral> CreateAsync(ServiceReferral entity, CancellationToken ct = default)
        {
            _context.ServiceReferrals.Add(entity);
            await _context.SaveChangesAsync(ct);

            // re-load with includes so the mapper has the lookup names
            var reloaded = await _context.ServiceReferrals
                .Include(r => r.Urgency)
                .Include(r => r.FollowUpSchedule)
                .FirstAsync(r => r.Id == entity.Id, ct);
            await EnrichAsync(new List<ServiceReferral> { reloaded }, ct);
            return reloaded;
        }

        public async Task<ServiceReferral> UpdateAsync(ServiceReferral entity, CancellationToken ct = default)
        {
            _context.ServiceReferrals.Update(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _context.ServiceReferrals.FindAsync(new object?[] { id }, ct);
            if (entity != null && !entity.IsDeleted)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<Guid?> GetHealthCampIdForAssignmentAsync(Guid serviceAssignmentId, CancellationToken ct = default)
        {
            var assignment = await _context.HealthCampServiceAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == serviceAssignmentId, ct);
            return assignment?.HealthCampId;
        }

        // Hydrates ServiceProviderFullName + ResolvedServiceName from User and Service tables.
        // Two batched lookups, no N+1.
        private async Task EnrichAsync(IList<ServiceReferral> referrals, CancellationToken ct)
        {
            if (referrals.Count == 0) return;

            var providerIds = referrals.Select(r => r.ServiceProviderId).Distinct().ToList();
            var providers = await _context.Users
                .AsNoTracking()
                .Where(u => providerIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToListAsync(ct);

            var assignmentIds = referrals.Select(r => r.ServiceAssignmentId).Distinct().ToList();
            var assignments = await _context.HealthCampServiceAssignments
                .AsNoTracking()
                .Where(a => assignmentIds.Contains(a.Id))
                .Select(a => new { a.Id, a.AssignmentId, a.ProfessionId })
                .ToListAsync(ct);

            var roleIds = assignments.Where(a => a.ProfessionId.HasValue).Select(a => a.ProfessionId!.Value).Distinct().ToList();
            var roles = await _context.SubcontractorRoles
                .AsNoTracking()
                .Where(r => roleIds.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, r => r.Name, ct);

            var assignmentToRole = assignments.ToDictionary(
                a => a.Id,
                a => a.ProfessionId.HasValue && roles.TryGetValue(a.ProfessionId.Value, out var rn) ? rn : string.Empty);

            var serviceIds = assignments.Select(a => a.AssignmentId).Distinct().ToList();
            var services = await _context.Services
                .AsNoTracking()
                .Where(s => serviceIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

            var assignmentToService = assignments.ToDictionary(
                a => a.Id,
                a => services.TryGetValue(a.AssignmentId, out var n) ? n : string.Empty);

            var providerById = providers.ToDictionary(
                p => p.Id,
                p => ((p.FirstName ?? string.Empty) + " " + (p.LastName ?? string.Empty)).Trim());

            foreach (var r in referrals)
            {
                r.ServiceProviderFullName = providerById.TryGetValue(r.ServiceProviderId, out var name) ? name : string.Empty;
                r.ResolvedServiceName = assignmentToService.TryGetValue(r.ServiceAssignmentId, out var sname) ? sname : string.Empty;
                r.Speciality = assignmentToRole.TryGetValue(r.ServiceAssignmentId, out var rname) ? rname : string.Empty;
            }
        }
    }
}
