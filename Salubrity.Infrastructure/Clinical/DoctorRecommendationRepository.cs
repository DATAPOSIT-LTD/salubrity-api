// File: Infrastructure/Repositories/Clinical/DoctorRecommendationRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Clinical;
using Salubrity.Domain.Entities.Clinical;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Clinical
{
    public class DoctorRecommendationRepository : IDoctorRecommendationRepository
    {
        private readonly AppDbContext _context;

        public DoctorRecommendationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DoctorRecommendation?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.DoctorRecommendations
                .Include(r => r.FollowUpRecommendation)
                .Include(r => r.RecommendationType)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);
        }

        public async Task<IReadOnlyList<DoctorRecommendation>> GetByPatientAsync(Guid patientId, CancellationToken ct = default)
        {
            return await _context.DoctorRecommendations
                .Include(r => r.FollowUpRecommendation)
                .Include(r => r.RecommendationType)
                .Where(r => r.PatientId == patientId && !r.IsDeleted)
                .ToListAsync(ct);
        }

        public async Task<DoctorRecommendation> CreateAsync(DoctorRecommendation entity, CancellationToken ct = default)
        {
            _context.DoctorRecommendations.Add(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task<DoctorRecommendation> UpdateAsync(DoctorRecommendation entity, CancellationToken ct = default)
        {
            _context.DoctorRecommendations.Update(entity);
            await _context.SaveChangesAsync(ct);
            return entity;
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _context.DoctorRecommendations.FindAsync(new object?[] { id }, ct);
            if (entity != null && !entity.IsDeleted)
            {
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(ct);
            }
        }

        public async Task<IReadOnlyList<DoctorRecommendation>> GetByHealthCampAsync(Guid healthCampId, CancellationToken ct = default)
        {
            return await _context.DoctorRecommendations
                .Include(r => r.FollowUpRecommendation)
                .Include(r => r.RecommendationType)
                .Where(r => r.HealthCampId == healthCampId && !r.IsDeleted)
                .ToListAsync(ct);
        }
    }
}
