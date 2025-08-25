using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HomepageOverview;
using Salubrity.Application.Interfaces.Repositories.HomepageOverview;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Join;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HomepageOverview
{
    public class HomepageOverviewRepository : IHomepageOverviewRepository
    {
        private readonly AppDbContext _context;

        public HomepageOverviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HomepageOverviewDto> GetHomepageOverviewAsync()
        {
            var totalPatients = await _context.Set<HealthCampParticipant>()
                .Where(p => p.PatientId != null)
                .Select(p => p.PatientId)
                .Distinct()
                .CountAsync();

            var completedCamps = await _context.Set<HealthCamp>()
                .Include(hc => hc.HealthCampStatus)
                .Where(hc => hc.HealthCampStatus != null && hc.HealthCampStatus.Name == "Completed")
                .CountAsync();

            var clients = await _context.Set<HealthCamp>()
                .Select(hc => hc.OrganizationId)
                .Distinct()
                .CountAsync();

            var servicesOffered = await _context.Set<Service>()
                .CountAsync();

            var overviewStats = new List<OverviewStatsDto>
            {
                new() { Name = "Total Patients", Value = totalPatients },
                new() { Name = "Completed Camps", Value = completedCamps },
                new() { Name = "Clients", Value = clients },
                new() { Name = "Services Offered", Value = servicesOffered }
            };

            var serviceUptake = await _context.Set<Service>()
                .Select(s => new ServiceUptakeDto
                {
                    Service = s.Name,
                    Uptake = _context.Set<HealthCampPackageItem>()
                        .Where(pi => pi.Id == s.Id)
                        .SelectMany(pi => pi.HealthCamp.Participants)
                        .Where(p => p.PatientId != null)
                        .Select(p => p.PatientId)
                        .Distinct()
                        .Count()
                })
                .ToListAsync();

            var today = DateTime.Now.Date;
            var upcomingCampDates = await _context.Set<HealthCamp>()
                .Where(hc => hc.StartDate > today)
                .Select(hc => hc.StartDate)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync();

            return new HomepageOverviewDto
            {
                OverviewStats = overviewStats,
                ServiceUptake = serviceUptake,
                UpcomingCampDates = upcomingCampDates
            };
        }
    }
}