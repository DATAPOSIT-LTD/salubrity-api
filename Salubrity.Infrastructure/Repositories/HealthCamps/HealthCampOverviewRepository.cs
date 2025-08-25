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
                .Select(hc => hc.OrganizationId)
                .Distinct()
                .CountAsync();

            var completedCamps = await _context.Set<HealthCamp>()
                .Include(hc => hc.HealthCampStatus)
                .Where(hc => hc.HealthCampStatus != null && hc.HealthCampStatus.Name == "Completed")
                .CountAsync();

            var today = DateTime.UtcNow.Date;
            var upcomingCamps = await _context.Set<HealthCamp>()
                .Where(hc => hc.StartDate > today)
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
    }
}
