using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Concierge;
using Salubrity.Application.Interfaces.Repositories.Concierge;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Concierge
{
    public class ConciergeRepository : IConciergeRepository
    {
        private readonly AppDbContext _db;
        public ConciergeRepository(AppDbContext db) => _db = db;


        public async Task<List<CampServiceStationInfoDto>> GetCampServiceStationsAsync(Guid campId, CancellationToken ct)
        {
            // Step 1: Load all assignments for the camp
            var assignments = await _db.HealthCampServiceAssignments
                .Include(a => a.Subcontractor).ThenInclude(s => s.User)
                .Where(a => a.HealthCampId == campId)
                .ToListAsync(ct);

            // Step 2: Resolve true ServiceId + ServiceName
            var stationInfo = new List<(Guid ServiceId, string ServiceName, Guid SubcontractorId, string SubcontractorName)>();

            foreach (var a in assignments)
            {
                Service? resolvedService = null;

                switch (a.AssignmentType)
                {
                    case PackageItemType.Service:
                        resolvedService = await _db.Services.FindAsync([a.AssignmentId], ct);
                        break;

                    case PackageItemType.ServiceCategory:
                        var category = await _db.ServiceCategories
                            .Include(c => c.Service)
                            .FirstOrDefaultAsync(c => c.Id == a.AssignmentId, ct);
                        resolvedService = category?.Service;
                        break;

                    case PackageItemType.ServiceSubcategory:
                        var subcategory = await _db.ServiceSubcategories
                            .Include(sc => sc.ServiceCategory).ThenInclude(c => c.Service)
                            .FirstOrDefaultAsync(sc => sc.Id == a.AssignmentId, ct);
                        resolvedService = subcategory?.ServiceCategory?.Service;
                        break;
                }

                if (resolvedService == null)
                    continue;

                stationInfo.Add((
                    ServiceId: resolvedService.Id,
                    ServiceName: resolvedService.Name,
                    SubcontractorId: a.Subcontractor.Id,
                    SubcontractorName: a.Subcontractor.User.FullName
                ));
            }

            // Step 3: Get all check-ins in queued status for the camp
            var queuedCheckIns = await _db.HealthCampStationCheckIns
                .Where(q => q.HealthCampId == campId && q.Status == "Queued")
                .ToListAsync(ct);

            // Step 4: Resolve check-in → assignment → service
            var serviceQueueCounts = new Dictionary<Guid, int>(); // ServiceId → count

            foreach (var checkIn in queuedCheckIns)
            {
                var assignment = assignments.FirstOrDefault(a => a.Id == checkIn.HealthCampServiceAssignmentId);
                if (assignment == null) continue;

                Guid? resolvedServiceId = assignment.AssignmentType switch
                {
                    PackageItemType.Service => assignment.AssignmentId,
                    PackageItemType.ServiceCategory => await _db.ServiceCategories
                        .Where(c => c.Id == assignment.AssignmentId)
                        .Select(c => c.ServiceId)
                        .FirstOrDefaultAsync(ct),
                    PackageItemType.ServiceSubcategory => await _db.ServiceSubcategories
                        .Where(sc => sc.Id == assignment.AssignmentId)
                        .Select(sc => sc.ServiceCategory.ServiceId)
                        .FirstOrDefaultAsync(ct),
                    _ => null
                };

                if (resolvedServiceId is null) continue;

                if (!serviceQueueCounts.ContainsKey(resolvedServiceId.Value))
                    serviceQueueCounts[resolvedServiceId.Value] = 0;

                serviceQueueCounts[resolvedServiceId.Value]++;
            }

            // Step 5: Group and return result
            var result = stationInfo
                .GroupBy(x => new { x.ServiceId, x.ServiceName })
                .Select(g => new CampServiceStationInfoDto
                {
                    ServiceId = g.Key.ServiceId,
                    ServiceName = g.Key.ServiceName,
                    QueueLength = serviceQueueCounts.TryGetValue(g.Key.ServiceId, out var count) ? count : 0,
                    AssignedSubcontractors = g
                        .Select(x => new AssignedSubcontractorDto
                        {
                            SubcontractorId = x.SubcontractorId,
                            SubcontractorName = x.SubcontractorName
                        })
                        .DistinctBy(s => s.SubcontractorId)
                        .ToList()
                })
                .ToList();

            return result;
        }

        public async Task<List<CampQueuePriorityDto>> GetCampQueuePrioritiesAsync(Guid campId, CancellationToken ct)
        {
            var checkIns = await _db.HealthCampStationCheckIns
                .Include(ci => ci.Participant).ThenInclude(p => p.User)
                .Include(ci => ci.Assignment).ThenInclude(a => a.AssignmentType)
                .Where(ci => ci.HealthCampId == campId && ci.Status == "Queued")
                .ToListAsync(ct);

            var result = checkIns.Select(ci => new CampQueuePriorityDto
            {
                ParticipantId = ci.Participant.Id,
                //PatientId = ci.Participant.PatientId ?? Guid.Empty,
                PatientName = ci.Participant.User.FullName,
                CurrentStation = ci.Assignment.AssignmentName,
                Priority = ci.Priority
            }).ToList();

            return result;
        }
    }
}
