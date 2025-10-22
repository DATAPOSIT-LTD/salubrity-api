using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Infrastructure.Persistence.Repositories.HealthCamps
{
    public class HealthCampParticipantPackageRepository : IHealthCampParticipantPackageRepository
    {
        private readonly AppDbContext _db;
        public HealthCampParticipantPackageRepository(AppDbContext db) => _db = db;

        public async Task<HealthCampParticipantPackage?> GetByParticipantIdAsync(Guid participantId, CancellationToken ct = default)
        {
            return await _db.HealthCampParticipantPackages
                .Include(x => x.HealthCampPackage)
                .ThenInclude(p => p.ServicePackage)
                .FirstOrDefaultAsync(x => x.ParticipantId == participantId && x.IsActive, ct);
        }

        public async Task AddAsync(HealthCampParticipantPackage entity, CancellationToken ct = default)
        {
            await _db.HealthCampParticipantPackages.AddAsync(entity, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(HealthCampParticipantPackage entity, CancellationToken ct = default)
        {
            _db.HealthCampParticipantPackages.Update(entity);
            await _db.SaveChangesAsync(ct);
        }
    }
}
