using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Concierge;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Concierge;
using Salubrity.Domain.Entities.HealthCamps;
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
                .Include(ci => ci.Assignment)
                .Where(ci => ci.HealthCampId == campId && (ci.Status == CampQueueStatus.Queued || ci.Status == CampQueueStatus.InService))
                .OrderByDescending(ci => ci.Priority)
                .ThenBy(ci => ci.CreatedAt)
                .ToListAsync(ct);

            var result = new List<CampQueuePriorityDto>();

            foreach (var ci in checkIns)
            {
                string stationName = "[Unknown Service]";
                if (ci.Assignment != null)
                {
                    if (ci.Assignment.AssignmentType == PackageItemType.Service)
                    {
                        stationName = await _db.Set<Service>()
                                          .Where(s => s.Id == ci.Assignment.AssignmentId)
                                          .Select(s => s.Name)
                                          .FirstOrDefaultAsync(ct) ?? "[Unknown Service]";
                    }
                    else if (ci.Assignment.AssignmentType == PackageItemType.ServiceCategory)
                    {
                        stationName = await _db.Set<ServiceCategory>()
                                          .Where(c => c.Id == ci.Assignment.AssignmentId)
                                          .Select(c => c.Name)
                                          .FirstOrDefaultAsync(ct) ?? "[Unknown Category]";
                    }
                    else if (ci.Assignment.AssignmentType == PackageItemType.ServiceSubcategory)
                    {
                        stationName = await _db.Set<ServiceSubcategory>()
                                          .Where(sc => sc.Id == ci.Assignment.AssignmentId)
                                          .Select(sc => sc.Name)
                                          .FirstOrDefaultAsync(ct) ?? "[Unknown Subcategory]";
                    }
                }

                result.Add(new CampQueuePriorityDto
                {
                    CheckInId = ci.Id,
                    ParticipantId = ci.Participant.Id,
                    PatientName = ci.Participant.User.FullName,
                    CurrentStation = stationName,
                    Priority = ci.Priority
                });
            }

            return result;
        }

        public async Task<List<CampServiceStationWithQueueDto>> GetCampServiceStationsWithQueueAsync(Guid campId, CancellationToken ct)
        {
            var serviceAssignments = await _db.Set<HealthCampServiceAssignment>()
                .Where(a => a.HealthCampId == campId)
                .Include(a => a.Subcontractor)
                    .ThenInclude(s => s!.User)
                .Select(a => new
                {
                    a.Id,
                    a.AssignmentId,
                    a.AssignmentType,
                    SubcontractorName = a.Subcontractor != null && a.Subcontractor.User != null
                        ? a.Subcontractor.User.FirstName + " " + a.Subcontractor.User.LastName
                        : "[Unassigned]"
                })
                .ToListAsync(ct);

            var result = new List<CampServiceStationWithQueueDto>();
            var now = DateTime.UtcNow;

            foreach (var assignment in serviceAssignments)
            {
                string stationName = "[Unknown Service]";
                if (assignment.AssignmentType == PackageItemType.Service)
                {
                    stationName = await _db.Set<Service>()
                                      .Where(s => s.Id == assignment.AssignmentId)
                                      .Select(s => s.Name)
                                      .FirstOrDefaultAsync(ct) ?? "[Unknown Service]";
                }
                else if (assignment.AssignmentType == PackageItemType.ServiceCategory)
                {
                    stationName = await _db.Set<ServiceCategory>()
                                      .Where(c => c.Id == assignment.AssignmentId)
                                      .Select(c => c.Name)
                                      .FirstOrDefaultAsync(ct) ?? "[Unknown Category]";
                }
                else if (assignment.AssignmentType == PackageItemType.ServiceSubcategory)
                {
                    stationName = await _db.Set<ServiceSubcategory>()
                                      .Where(sc => sc.Id == assignment.AssignmentId)
                                      .Select(sc => sc.Name)
                                      .FirstOrDefaultAsync(ct) ?? "[Unknown Subcategory]";
                }

                var queue = await _db.Set<HealthCampStationCheckIn>()
                    .Where(c => c.HealthCampServiceAssignmentId == assignment.Id && c.Status == "Queued")
                    .Include(c => c.Participant)
                        .ThenInclude(p => p.User)
                    .OrderBy(c => c.CreatedAt)
                    .Select(c => new QueuedParticipantDto
                    {
                        PatientName = c.Participant != null && c.Participant.User != null
                            ? c.Participant.User.FirstName + " " + c.Participant.User.LastName
                            : "[Unknown Patient]",
                        QueueTime = FormatTimeSpan(now - c.CreatedAt)
                    })
                    .ToListAsync(ct);

                result.Add(new CampServiceStationWithQueueDto
                {
                    AssignmentId = assignment.Id,
                    ServiceStation = stationName,
                    AssignedSubcontractor = assignment.SubcontractorName,
                    QueueLength = queue.Count,
                    Queue = queue
                });
            }

            return result;
        }

        private static string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalMinutes >= 60)
            {
                return $"{(int)timeSpan.TotalHours}h {(int)timeSpan.Minutes % 60}m";
            }
            return $"{(int)timeSpan.TotalMinutes}m";
        }

        public async Task<PatientDetailDto?> GetPatientDetailByIdAsync(Guid patientId, CancellationToken ct = default)
        {
            var query = from patient in _db.Patients
                        join user in _db.Users on patient.UserId equals user.Id
                        where patient.Id == patientId
                        select new
                        {
                            Patient = patient,
                            User = user,
                            Gender = user.Gender,
                            Organization = user.Organization,
                            LatestIntakeFormResponse = _db.IntakeFormResponses
                                .Where(ifr => ifr.PatientId == patient.Id)
                                .OrderByDescending(ifr => ifr.CreatedAt)
                                .FirstOrDefault()
                        };

            var result = await query.AsNoTracking().FirstOrDefaultAsync(ct);

            if (result == null)
            {
                return null;
            }

            int? age = null;
            if (result.User.DateOfBirth.HasValue)
            {
                var today = DateTime.Today;
                age = today.Year - result.User.DateOfBirth.Value.Year;
                if (result.User.DateOfBirth.Value.Date > today.AddYears(-age.Value))
                {
                    age--;
                }
            }

            return new PatientDetailDto
            {
                ProfilePictureUrl = result.User.ProfileImage,
                FullName = $"{result.User.FirstName} {result.User.LastName}",
                Phone = result.User.Phone,
                Email = result.User.Email,
                Gender = result.Gender?.Name,
                Age = age,
                Organization = result.Organization?.BusinessName,
            };
        }
    }
}
