using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Join;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HealthCamps
{
    public class HealthCampOverviewRepository : IHealthCampOverviewRepository
    {
        private readonly AppDbContext _context;

        public HealthCampOverviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCampOverviewDto> GetHealthCampOverviewAsync()
        {
            var onboardedOrganizations = await _context.Set<HealthCamp>()
                .Where(hc => !hc.IsDeleted)
                .Select(hc => hc.OrganizationId)
                .Distinct()
                .CountAsync();

            // Align with camp list "Complete" tab: IsLaunched and (EndDate ?? StartDate) < today
            var today = DateTime.UtcNow.Date;
            var completedCamps = await _context.Set<HealthCamp>()
                .Where(hc => !hc.IsDeleted
                    && hc.IsLaunched
                    && (hc.EndDate ?? hc.StartDate) < today)
                .CountAsync();

            // Align with camp list "Upcoming" tab: IsLaunched and (EndDate ?? StartDate) >= today
            var upcomingCamps = await _context.Set<HealthCamp>()
                .Where(hc => !hc.IsDeleted
                    && hc.IsLaunched
                    && (hc.EndDate ?? hc.StartDate) >= today)
                .CountAsync();

            var totalPatients = await _context.Set<HealthCampParticipant>()
                .Where(p => p.PatientId != null)
                .Select(p => p.PatientId)
                .Distinct()
                .CountAsync();

            return new HealthCampOverviewDto
            {
                OnboardedOrganizations = onboardedOrganizations,
                CompletedCamps = completedCamps,
                UpcomingCamps = upcomingCamps,
                TotalPatients = totalPatients
            };
        }

        public async Task<PatientCampOverviewDto> GetPatientCampOverviewAsync(Guid patientId, CancellationToken ct = default)
        {
            var today = DateTime.UtcNow.Date;

            // Camps attended = completed camps this patient participated in (same date logic as camp list)
            var campsAttended = await _context.Set<HealthCampParticipant>()
                .Where(p => p.PatientId == patientId
                    && !p.HealthCamp.IsDeleted
                    && p.HealthCamp.IsLaunched
                    && (p.HealthCamp.EndDate ?? p.HealthCamp.StartDate) < today)
                .CountAsync(ct);

            // Upcoming camps = camps this patient is registered for that have not ended yet
            var upcomingCamps = await _context.Set<HealthCampParticipant>()
                .Where(p => p.PatientId == patientId
                    && !p.HealthCamp.IsDeleted
                    && p.HealthCamp.IsLaunched
                    && (p.HealthCamp.EndDate ?? p.HealthCamp.StartDate) >= today)
                .CountAsync(ct);

            return new PatientCampOverviewDto
            {
                CampsAttended = campsAttended,
                UpcomingCamps = upcomingCamps
            };
        }
    }
}
