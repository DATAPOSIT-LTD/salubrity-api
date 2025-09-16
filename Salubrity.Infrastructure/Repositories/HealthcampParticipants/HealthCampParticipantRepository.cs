using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Domain.Entities.Join;

namespace Salubrity.Infrastructure.Repositories
{
    public class HealthCampParticipantRepository : IHealthCampParticipantRepository
    {
        private readonly AppDbContext _context;

        public HealthCampParticipantRepository(AppDbContext context)
        {
            _context = context;
        }



        public async Task<Guid?> GetPatientIdByParticipantIdAsync(Guid participantId, CancellationToken ct = default)
        {
            // Step 1: Get the participant and their UserId
            var userId = await _context.HealthCampParticipants
                .Where(p => p.Id == participantId && !p.IsDeleted)
                .Select(p => p.UserId)
                .FirstOrDefaultAsync(ct);

            if (userId == Guid.Empty)
                return null;

            // Step 2: Find the patient by UserId
            return await _context.Patients
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> IsParticipantLinkedToCampAsync(Guid campId, Guid userId, CancellationToken ct = default)
        {
            return await _context.HealthCampParticipants
                .AnyAsync(p => p.HealthCampId == campId && p.UserId == userId && !p.IsDeleted, ct);
        }
        public async Task AddParticipantAsync(HealthCampParticipant participant, CancellationToken ct = default)
        {
            await _context.HealthCampParticipants.AddAsync(participant, ct);
            await _context.SaveChangesAsync(ct);
        }
        public async Task<HealthCampParticipant?> GetParticipantAsync(Guid campId, Guid participantId, CancellationToken ct = default)
        {
            return await _context.Set<HealthCampParticipant>()
                .FirstOrDefaultAsync(p => p.HealthCampId == campId && p.Id == participantId && !p.IsDeleted, ct);
        }

        public async Task UpdateParticipantAsync(HealthCampParticipant participant, CancellationToken ct = default)
        {
            _context.Set<HealthCampParticipant>().Update(participant);
            await _context.SaveChangesAsync(ct);
        }

        public async Task<Guid?> GetParticipantIdByPatientIdAsync(Guid patientId, CancellationToken ct = default)
        {
            // Step 1: get UserId from patient
            var userId = await _context.Patients
                .Where(p => p.Id == patientId && !p.IsDeleted)
                .Select(p => p.UserId)
                .FirstOrDefaultAsync(ct);

            if (userId == Guid.Empty)
                return null;

            // Step 2: get participant by UserId
            return await _context.HealthCampParticipants
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .Select(p => p.Id)
                .FirstOrDefaultAsync(ct);
        }

        public async Task<HealthCampParticipant?> GetParticipantWithBillingStatusAsync(Guid campId, Guid participantId, CancellationToken ct = default)
        {
            return await _context.HealthCampParticipants
                .Include(p => p.BillingStatus)
                .FirstOrDefaultAsync(p => p.HealthCampId == campId && p.Id == participantId, ct);
        }

        public async Task<HealthCampParticipant?> GetParticipantWithBillingStatusByIdAsync(Guid participantId, CancellationToken ct = default)
        {
            return await _context.HealthCampParticipants
                .Include(p => p.BillingStatus)
                .FirstOrDefaultAsync(p => p.Id == participantId, ct);
        }
    }


}
