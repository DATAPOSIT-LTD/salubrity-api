using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Concierge;
using Salubrity.Application.Interfaces.Repositories.Concierge;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Concierge
{
    public class ConciergeRepository : IConciergeRepository
    {
        private readonly AppDbContext _db;
        public ConciergeRepository(AppDbContext db) => _db = db;


        public async Task<List<CampServiceStationInfoDto>> GetCampServiceStationsAsync(Guid campId, CancellationToken ct)
        {
            // Get all service assignments for the camp, including service and subcontractor
            var assignments = await _db.HealthCampServiceAssignments
                .Include(a => a.Service)
                .Include(a => a.Subcontractor).ThenInclude(s => s.User)
                .Where(a => a.HealthCampId == campId)
                .ToListAsync(ct);

            // Group by service
            var serviceGroups = assignments
                .GroupBy(a => new { a.Service.Id, a.Service.Name })
                .Select(g => new CampServiceStationInfoDto
                {
                    ServiceId = g.Key.Id,
                    ServiceName = g.Key.Name,
                    QueueLength = _db.HealthCampStationCheckIns.Count(q =>
                        q.HealthCampId == campId &&
                        q.Assignment.ServiceId == g.Key.Id &&
                        q.Status == "Queued"
                    ),
                    AssignedSubcontractors = g.Select(a => new AssignedSubcontractorDto
                    {
                        SubcontractorId = a.Subcontractor.Id,
                        SubcontractorName = a.Subcontractor.User.FullName
                    }).DistinctBy(s => s.SubcontractorId).ToList()
                });

            return serviceGroups.ToList();
        }
    }
}
