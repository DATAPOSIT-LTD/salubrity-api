using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Domain.Entities.Join;
using Salubrity.Infrastructure.Persistence;

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



        // ----------------------------------------------------
        // Resolve services assigned to a camp
        // ----------------------------------------------------
        public async Task<List<Guid>> GetServiceIdsForCampAsync(
            Guid campId,
            CancellationToken ct)
        {
            return await _context.HealthCampServiceAssignments
                .Where(a =>
                    a.HealthCampId == campId &&
                    !a.IsDeleted &&
                    a.AssignmentType == PackageItemType.Service)
                .Select(a => a.AssignmentId)
                .Distinct()
                .ToListAsync(ct);
        }

        // ----------------------------------------------------
        // Get form responses tied to patient + camp services
        // ----------------------------------------------------
        public async Task<List<IntakeFormResponse>> GetFormResponsesForPatientAndServicesAsync(
            Guid patientId,
            List<Guid> serviceIds,
            CancellationToken ct)
        {
            if (serviceIds.Count == 0)
                return new List<IntakeFormResponse>();

            return await _context.IntakeFormResponses
                .Include(r => r.FieldResponses)
                .Where(r =>
                    r.PatientId == patientId &&
                    serviceIds.Contains(r.ResolvedServiceId) &&
                    !r.IsDeleted)
                .ToListAsync(ct);
        }

        // ----------------------------------------------------
        // Unit-of-work commit
        // ----------------------------------------------------
        public async Task SaveChangesAsync(CancellationToken ct)
        {
            await _context.SaveChangesAsync(ct);
        }

        public async Task<HealthCampParticipant?> GetParticipantWithDemographicsAsync(Guid participantId, CancellationToken ct = default)
        {
            // Note: HealthCampParticipants.PatientId may be null in legacy data.
            // The patient profile is reliably found via User → Patients (Patients.UserId = User.Id).
            var participant = await _context.HealthCampParticipants
                .AsNoTracking()
                .Include(p => p.HealthCamp)
                .Include(p => p.User).ThenInclude(u => u.Gender)
                .Include(p => p.Patient!).ThenInclude(pa => pa.PrimaryOrganization)
                .FirstOrDefaultAsync(p => p.Id == participantId, ct);

            if (participant is null) return null;

            // Fallback: load Patient via UserId if not directly linked
            if (participant.Patient is null && participant.User is not null)
            {
                var patient = await _context.Patients
                    .AsNoTracking()
                    .Include(pa => pa.PrimaryOrganization)
                    .FirstOrDefaultAsync(pa => pa.UserId == participant.UserId, ct);
                if (patient is not null)
                {
                    participant.Patient = patient;
                    participant.PatientId = patient.Id;
                }
            }

            return participant;
        }

        public async Task<Guid?> GetParticipantIdByUserAndCampAsync(Guid userId, Guid campId, CancellationToken ct = default)
        {
            return await _context.HealthCampParticipants
                .AsNoTracking()
                .Where(p => p.UserId == userId && p.HealthCampId == campId)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(ct);
        }
    }


}
