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
            var assignments = await _db.HealthCampServiceAssignments
                .Include(a => a.Subcontractor).ThenInclude(s => s.User)
                .Where(a => a.HealthCampId == campId)
                .ToListAsync(ct);

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

                if (resolvedService == null) continue;

                stationInfo.Add((
                    ServiceId: resolvedService.Id,
                    ServiceName: resolvedService.Name,
                    SubcontractorId: a.Subcontractor.Id,
                    SubcontractorName: a.Subcontractor.User.FullName
                ));
            }

            var queuedCheckIns = await _db.HealthCampStationCheckIns
                .Where(q => q.HealthCampId == campId && q.Status == "Queued")
                .ToListAsync(ct);

            var serviceQueueCounts = new Dictionary<Guid, int>();

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

            return stationInfo
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
        }

        public async Task<List<CampQueuePriorityDto>> GetCampQueuePrioritiesAsync(Guid campId, CancellationToken ct)
        {
            var checkIns = await _db.HealthCampStationCheckIns
                .Include(ci => ci.Participant).ThenInclude(p => p.User)
                .Include(ci => ci.Assignment)
                .Where(ci => ci.HealthCampId == campId &&
                             (ci.Status == CampQueueStatus.Queued || ci.Status == CampQueueStatus.InService))
                .OrderByDescending(ci => ci.Priority)
                .ThenBy(ci => ci.CreatedAt)
                .ToListAsync(ct);

            var result = new List<CampQueuePriorityDto>();

            foreach (var ci in checkIns)
            {
                string stationName = "[Unknown Service]";
                if (ci.Assignment != null)
                {
                    stationName = ci.Assignment.AssignmentType switch
                    {
                        PackageItemType.Service => await _db.Set<Service>()
                            .Where(s => s.Id == ci.Assignment.AssignmentId).Select(s => s.Name)
                            .FirstOrDefaultAsync(ct) ?? "[Unknown Service]",
                        PackageItemType.ServiceCategory => await _db.Set<ServiceCategory>()
                            .Where(c => c.Id == ci.Assignment.AssignmentId).Select(c => c.Name)
                            .FirstOrDefaultAsync(ct) ?? "[Unknown Category]",
                        PackageItemType.ServiceSubcategory => await _db.Set<ServiceSubcategory>()
                            .Where(sc => sc.Id == ci.Assignment.AssignmentId).Select(sc => sc.Name)
                            .FirstOrDefaultAsync(ct) ?? "[Unknown Subcategory]",
                        _ => "[Unknown Service]"
                    };
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
                .Include(a => a.Subcontractor).ThenInclude(s => s!.User)
                .Select(a => new
                {
                    a.Id, a.AssignmentId, a.AssignmentType,
                    SubcontractorName = a.Subcontractor != null && a.Subcontractor.User != null
                        ? a.Subcontractor.User.FirstName + " " + a.Subcontractor.User.LastName
                        : "[Unassigned]"
                })
                .ToListAsync(ct);

            var result = new List<CampServiceStationWithQueueDto>();
            var now = DateTime.UtcNow;

            foreach (var assignment in serviceAssignments)
            {
                string stationName = assignment.AssignmentType switch
                {
                    PackageItemType.Service => await _db.Set<Service>()
                        .Where(s => s.Id == assignment.AssignmentId).Select(s => s.Name)
                        .FirstOrDefaultAsync(ct) ?? "[Unknown Service]",
                    PackageItemType.ServiceCategory => await _db.Set<ServiceCategory>()
                        .Where(c => c.Id == assignment.AssignmentId).Select(c => c.Name)
                        .FirstOrDefaultAsync(ct) ?? "[Unknown Category]",
                    PackageItemType.ServiceSubcategory => await _db.Set<ServiceSubcategory>()
                        .Where(sc => sc.Id == assignment.AssignmentId).Select(sc => sc.Name)
                        .FirstOrDefaultAsync(ct) ?? "[Unknown Subcategory]",
                    _ => "[Unknown Service]"
                };

                var queue = await _db.Set<HealthCampStationCheckIn>()
                    .Where(c => c.HealthCampServiceAssignmentId == assignment.Id && c.Status == "Queued")
                    .Include(c => c.Participant).ThenInclude(p => p.User)
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

        private static string FormatTimeSpan(TimeSpan ts) =>
            ts.TotalMinutes >= 60
                ? $"{(int)ts.TotalHours}h {ts.Minutes % 60}m"
                : $"{(int)ts.TotalMinutes}m";

        public async Task<PatientDetailDto?> GetPatientDetailByIdAsync(Guid patientId, CancellationToken ct = default)
        {
            var result = await (
                from patient in _db.Patients
                join user in _db.Users on patient.UserId equals user.Id
                where patient.Id == patientId
                select new { Patient = patient, User = user, user.Gender, user.Organization }
            ).AsNoTracking().FirstOrDefaultAsync(ct);

            if (result == null) return null;

            int? age = null;
            if (result.User.DateOfBirth.HasValue)
            {
                var today = DateTime.Today;
                age = today.Year - result.User.DateOfBirth.Value.Year;
                if (result.User.DateOfBirth.Value.Date > today.AddYears(-age.Value)) age--;
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

        // ── Live camp KPI stats ───────────────────────────────────────────────
        public async Task<CampLiveStatsDto> GetCampLiveStatsAsync(Guid campId, CancellationToken ct)
        {
            var registered = await _db.HealthCampParticipants
                .CountAsync(p => p.HealthCampId == campId && !p.IsDeleted, ct);

            var activeParticipantIds = await _db.HealthCampStationCheckIns
                .Where(ci => ci.HealthCampId == campId && !ci.IsDeleted &&
                             (ci.Status == "Queued" || ci.Status == "InService"))
                .Select(ci => ci.HealthCampParticipantId)
                .Distinct().ToListAsync(ct);

            var participantsWithCompleted = await _db.HealthCampStationCheckIns
                .Where(ci => ci.HealthCampId == campId && !ci.IsDeleted && ci.Status == "Completed")
                .Select(ci => ci.HealthCampParticipantId)
                .Distinct().ToListAsync(ct);

            var activeSet = new HashSet<Guid>(activeParticipantIds);

            var referred = await _db.ServiceReferrals
                .Where(r => r.HealthCampId == campId && !r.IsDeleted)
                .Select(r => r.ParticipantId)
                .Distinct().CountAsync(ct);

            return new CampLiveStatsDto
            {
                Registered = registered,
                InStation = activeParticipantIds.Count,
                Completed = participantsWithCompleted.Count(id => !activeSet.Contains(id)),
                Referred = referred,
            };
        }

        // ── Participant quick search ──────────────────────────────────────────
        public async Task<List<ParticipantSearchResultDto>> SearchParticipantsAsync(Guid campId, string query, CancellationToken ct)
        {
            var q = (query ?? string.Empty).ToLowerInvariant().Trim();

            var participants = await (
                from p in _db.HealthCampParticipants.AsNoTracking()
                join u in _db.Users.AsNoTracking() on p.UserId equals u.Id
                where p.HealthCampId == campId && !p.IsDeleted
                      && (string.IsNullOrEmpty(q) ||
                          (u.FirstName + " " + u.LastName).ToLower().Contains(q) ||
                          u.Phone.ToLower().Contains(q) ||
                          u.Email.ToLower().Contains(q))
                select new
                {
                    ParticipantId = p.Id,
                    u.FirstName, u.LastName,
                    GenderName = u.Gender != null ? u.Gender.Name : null,
                    OrgName = u.Organization != null ? u.Organization.BusinessName : null,
                }
            ).Take(20).ToListAsync(ct);

            if (participants.Count == 0) return [];

            var participantIds = participants.Select(p => p.ParticipantId).ToList();

            var allCheckIns = await _db.HealthCampStationCheckIns
                .AsNoTracking()
                .Where(ci => ci.HealthCampId == campId && !ci.IsDeleted &&
                             participantIds.Contains(ci.HealthCampParticipantId))
                .Include(ci => ci.Assignment)
                .OrderBy(ci => ci.CreatedAt)
                .ToListAsync(ct);

            var assignmentIds = allCheckIns.Select(ci => ci.HealthCampServiceAssignmentId).Distinct().ToList();
            var assignments = await _db.HealthCampServiceAssignments.AsNoTracking()
                .Where(a => assignmentIds.Contains(a.Id)).ToListAsync(ct);

            var stationNameCache = new Dictionary<Guid, string>();
            foreach (var a in assignments)
            {
                stationNameCache[a.Id] = a.AssignmentType switch
                {
                    PackageItemType.Service => await _db.Services
                        .Where(s => s.Id == a.AssignmentId).Select(s => s.Name).FirstOrDefaultAsync(ct) ?? "Unknown",
                    PackageItemType.ServiceCategory => await _db.ServiceCategories
                        .Where(c => c.Id == a.AssignmentId).Select(c => c.Name).FirstOrDefaultAsync(ct) ?? "Unknown",
                    PackageItemType.ServiceSubcategory => await _db.ServiceSubcategories
                        .Where(sc => sc.Id == a.AssignmentId).Select(sc => sc.Name).FirstOrDefaultAsync(ct) ?? "Unknown",
                    _ => "Unknown"
                };
            }

            var result = new List<ParticipantSearchResultDto>();

            foreach (var p in participants)
            {
                var checkIns = allCheckIns
                    .Where(ci => ci.HealthCampParticipantId == p.ParticipantId)
                    .OrderBy(ci => ci.CreatedAt).ToList();

                var activeCheckIn = checkIns.FirstOrDefault(ci => ci.Status == "Queued" || ci.Status == "InService");

                result.Add(new ParticipantSearchResultDto
                {
                    ParticipantId = p.ParticipantId,
                    FullName = $"{p.FirstName} {p.LastName}",
                    Gender = p.GenderName,
                    Organization = p.OrgName,
                    CurrentStatus = activeCheckIn?.Status
                        ?? (checkIns.Any(ci => ci.Status == "Completed") ? "Completed" : "Registered"),
                    CurrentStation = activeCheckIn != null
                        ? stationNameCache.GetValueOrDefault(activeCheckIn.HealthCampServiceAssignmentId)
                        : null,
                    Journey = checkIns.Select(ci => new ParticipantStationJourneyDto
                    {
                        StationName = stationNameCache.GetValueOrDefault(ci.HealthCampServiceAssignmentId, "Unknown"),
                        Status = ci.Status,
                        StartedAt = ci.StartedAt,
                        FinishedAt = ci.FinishedAt,
                        DurationMinutes = ci.StartedAt.HasValue && ci.FinishedAt.HasValue
                            ? (int)(ci.FinishedAt.Value - ci.StartedAt.Value).TotalMinutes : null,
                    }).ToList(),
                });
            }

            return result;
        }

        // ── Station bottleneck view ───────────────────────────────────────────
        public async Task<List<StationBottleneckDto>> GetStationBottlenecksAsync(Guid campId, CancellationToken ct)
        {
            var assignments = await _db.HealthCampServiceAssignments
                .Include(a => a.Subcontractor).ThenInclude(s => s.User)
                .Where(a => a.HealthCampId == campId)
                .ToListAsync(ct);

            var now = DateTimeOffset.UtcNow;

            var checkIns = await _db.HealthCampStationCheckIns.AsNoTracking()
                .Where(ci => ci.HealthCampId == campId && !ci.IsDeleted)
                .ToListAsync(ct);

            var serviceMap = new Dictionary<Guid, (string Name, List<Guid> AssignmentIds, List<AssignedSubcontractorDto> Subs)>();

            foreach (var a in assignments)
            {
                Guid? serviceId = null;
                string? serviceName = null;

                switch (a.AssignmentType)
                {
                    case PackageItemType.Service:
                        var svc = await _db.Services.FindAsync([a.AssignmentId], ct);
                        serviceId = svc?.Id; serviceName = svc?.Name;
                        break;
                    case PackageItemType.ServiceCategory:
                        var cat = await _db.ServiceCategories.Include(c => c.Service)
                            .FirstOrDefaultAsync(c => c.Id == a.AssignmentId, ct);
                        serviceId = cat?.Service?.Id; serviceName = cat?.Service?.Name;
                        break;
                    case PackageItemType.ServiceSubcategory:
                        var sub = await _db.ServiceSubcategories
                            .Include(sc => sc.ServiceCategory).ThenInclude(c => c.Service)
                            .FirstOrDefaultAsync(sc => sc.Id == a.AssignmentId, ct);
                        serviceId = sub?.ServiceCategory?.Service?.Id;
                        serviceName = sub?.ServiceCategory?.Service?.Name;
                        break;
                }

                if (serviceId == null || serviceName == null) continue;

                if (!serviceMap.TryGetValue(serviceId.Value, out var entry))
                {
                    entry = (serviceName, [], []);
                    serviceMap[serviceId.Value] = entry;
                }

                entry.AssignmentIds.Add(a.Id);

                if (a.Subcontractor?.User != null)
                {
                    var subDto = new AssignedSubcontractorDto
                    {
                        SubcontractorId = a.Subcontractor.Id,
                        SubcontractorName = a.Subcontractor.User.FullName,
                    };
                    if (!entry.Subs.Any(s => s.SubcontractorId == subDto.SubcontractorId))
                        entry.Subs.Add(subDto);
                }
            }

            var result = new List<StationBottleneckDto>();

            foreach (var (serviceId, (serviceName, assignmentIds, subs)) in serviceMap)
            {
                var stationCIs = checkIns.Where(ci => assignmentIds.Contains(ci.HealthCampServiceAssignmentId)).ToList();
                var queued = stationCIs.Where(ci => ci.Status == "Queued").ToList();
                var completed = stationCIs.Where(ci =>
                    ci.Status == "Completed" && ci.StartedAt.HasValue && ci.FinishedAt.HasValue).ToList();

                result.Add(new StationBottleneckDto
                {
                    ServiceId = serviceId,
                    ServiceName = serviceName,
                    QueueLength = queued.Count,
                    AvgServiceMinutes = completed.Count > 0
                        ? Math.Round(completed.Average(ci => (ci.FinishedAt!.Value - ci.StartedAt!.Value).TotalMinutes), 1)
                        : null,
                    MaxWaitMinutes = queued.Count > 0
                        ? Math.Round(queued.Max(ci => (now - ci.CreatedAt).TotalMinutes), 1)
                        : null,
                    AssignedSubcontractors = subs,
                });
            }

            return result.OrderByDescending(s => s.QueueLength).ToList();
        }

        // ── Registration timeline (arrivals per hour, EAT = UTC+3) ───────────
        public async Task<RegistrationTimelineDto> GetRegistrationTimelineAsync(Guid campId, CancellationToken ct)
        {
            var eat = TimeSpan.FromHours(3);

            var timestamps = await _db.HealthCampParticipants.AsNoTracking()
                .Where(p => p.HealthCampId == campId && !p.IsDeleted)
                .Select(p => p.CreatedAt)
                .ToListAsync(ct);

            if (timestamps.Count == 0)
                return new RegistrationTimelineDto();

            var grouped = timestamps
                .Select(t => new DateTimeOffset(t, TimeSpan.Zero).ToOffset(eat))
                .GroupBy(t => new DateTimeOffset(t.Year, t.Month, t.Day, t.Hour, 0, 0, eat))
                .OrderBy(g => g.Key)
                .Select(g => new { Hour = g.Key, Count = g.Count() })
                .ToList();

            var first = grouped.First().Hour;
            var last = grouped.Last().Hour;
            var slots = new List<RegistrationSlotDto>();
            int cumulative = 0;

            for (var h = first; h <= last; h = h.AddHours(1))
            {
                var match = grouped.FirstOrDefault(g => g.Hour == h);
                int count = match?.Count ?? 0;
                cumulative += count;
                slots.Add(new RegistrationSlotDto
                {
                    Label = h.ToString("HH:mm"),
                    Count = count,
                    Cumulative = cumulative,
                });
            }

            var peak = grouped.MaxBy(g => g.Count)!;

            return new RegistrationTimelineDto
            {
                Slots = slots,
                TotalRegistered = timestamps.Count,
                PeakCount = peak.Count,
                PeakLabel = peak.Hour.ToString("HH:mm"),
            };
        }
    }
}
