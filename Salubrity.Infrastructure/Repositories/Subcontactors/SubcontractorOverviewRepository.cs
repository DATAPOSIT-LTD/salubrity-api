using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Subcontractor;
using Salubrity.Application.Interfaces.Repositories.Subcontactors;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Subcontractor;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Subcontactors
{
    public class SubcontractorOverviewRepository : ISubcontractorOverviewRepository
    {
        private readonly AppDbContext _context;

        public SubcontractorOverviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SubcontractorOverviewDto> GetSubcontractorOverviewAsync()
        {
            var totalSubcontractors = await _context.Set<Subcontractor>().CountAsync();

            var activeSubcontractors = await _context.Set<Subcontractor>()
                .Include(s => s.Status)
                .Where(s => s.Status != null && s.Status.Name == "Active")
                .CountAsync();

            var today = DateTime.UtcNow.Date;
            var upcomingCamps = await _context.Set<HealthCamp>()
                .Where(hc => hc.StartDate > today)
                .CountAsync();

            var completedCamps = await _context.Set<HealthCamp>()
                .Include(hc => hc.HealthCampStatus)
                .Where(hc => (hc.HealthCampStatus != null && hc.HealthCampStatus.Name == "Completed") || hc.EndDate < today)
                .CountAsync();

            return new SubcontractorOverviewDto
            {
                TotalSubcontractors = totalSubcontractors,
                ActiveSubcontractors = activeSubcontractors,
                UpcomingCamps = upcomingCamps,
                CompletedCamps = completedCamps
            };
        }
    }
}
