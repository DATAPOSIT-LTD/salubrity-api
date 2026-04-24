// File: Infrastructure/Repositories/Reporting/CorporateReportRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Reports;
using Salubrity.Application.Interfaces.Repositories.Reporting;
using Salubrity.Application.Services.Reporting.Reports;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Reporting;

public sealed class CorporateReportRepository : ICorporateReportRepository
{
    private readonly AppDbContext _db;

    public CorporateReportRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CorporateRawDataDto?> LoadAsync(Guid campId, CorporateReportFilters? filters = null, CancellationToken ct = default)
    {
        // Camp metadata + active package + client name
        var camp = await _db.HealthCamps
            .AsNoTracking()
            .Include(c => c.Organization)
            .Include(c => c.HealthCampPackages.Where(hp => hp.IsActive))
                .ThenInclude(hp => hp.ServicePackage)
            .FirstOrDefaultAsync(c => c.Id == campId, ct);
        if (camp is null) return null;

        var packageName = camp.HealthCampPackages
            .FirstOrDefault(hp => hp.IsActive)?.ServicePackage?.Name ?? string.Empty;

        // Compute age bucket bounds (UTC year-based approximation, fine for filters)
        var today = DateTime.UtcNow.Date;
        DateTime? minDob = null, maxDob = null;
        if (!string.IsNullOrWhiteSpace(filters?.AgeBucket))
        {
            (int lo, int hi) = filters!.AgeBucket switch
            {
                "18-30" => (18, 30),
                "31-45" => (31, 45),
                "46-60" => (46, 60),
                "60+"   => (60, 200),
                _ => (0, 200),
            };
            // Born between (today - hi years) and (today - lo years)
            minDob = today.AddYears(-(hi + 1)).AddDays(1);
            maxDob = today.AddYears(-lo);
        }

        // Participant gender + age filter
        var participantsQuery = _db.HealthCampParticipants
            .AsNoTracking()
            .Where(p => p.HealthCampId == campId)
            .Include(p => p.User).ThenInclude(u => u.Gender)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filters?.Gender))
            participantsQuery = participantsQuery.Where(p => p.User.Gender != null && p.User.Gender.Name == filters!.Gender);
        if (minDob.HasValue && maxDob.HasValue)
            participantsQuery = participantsQuery.Where(p =>
                p.User.DateOfBirth.HasValue
                && p.User.DateOfBirth.Value >= minDob.Value
                && p.User.DateOfBirth.Value <= maxDob.Value);

        var participantGenders = await participantsQuery
            .Select(p => p.User.Gender != null ? p.User.Gender.Name : null)
            .ToListAsync(ct);

        // Capture filtered participant ids for downstream queries
        var filteredParticipantIds = await participantsQuery
            .Where(p => p.PatientId.HasValue)
            .Select(p => p.PatientId!.Value)
            .ToListAsync(ct);

        var totalAttendees = participantGenders.Count;
        var female = participantGenders.Count(g => string.Equals(g, "Female", StringComparison.OrdinalIgnoreCase));
        var male = participantGenders.Count(g => string.Equals(g, "Male", StringComparison.OrdinalIgnoreCase));

        // Service stations
        var assignmentServiceIds = await _db.HealthCampServiceAssignments
            .AsNoTracking()
            .Where(a => a.HealthCampId == campId)
            .Select(a => a.AssignmentId)
            .Distinct()
            .ToListAsync(ct);

        var serviceNames = await _db.Services
            .AsNoTracking()
            .Where(s => assignmentServiceIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, ct);

        // Per-station: count distinct participants who submitted, broken by gender (% of that gender).
        var stationCompletion = new List<StationCompletionDto>();
        foreach (var serviceId in assignmentServiceIds)
        {
            var name = serviceNames.TryGetValue(serviceId, out var n) ? n : "Unknown";

            var submissionsQ = _db.IntakeFormResponses
                .AsNoTracking()
                .Where(r => r.HealthCampId == campId && r.SubmittedServiceId == serviceId && !r.IsDeleted);
            if (filters?.Day.HasValue == true)
            {
                var dayDate = camp.StartDate.Date.AddDays(filters.Day.Value - 1);
                submissionsQ = submissionsQ.Where(r => r.CreatedAt.Date == dayDate);
            }
            var submittedPatientIds = await submissionsQ
                .Where(r => filteredParticipantIds.Contains(r.PatientId))
                .Select(r => r.PatientId)
                .Distinct()
                .ToListAsync(ct);

            var subFem = await _db.HealthCampParticipants
                .AsNoTracking()
                .Where(p => p.HealthCampId == campId
                            && p.PatientId.HasValue
                            && submittedPatientIds.Contains(p.PatientId.Value))
                .Where(p => p.User.Gender != null && p.User.Gender.Name == "Female")
                .CountAsync(ct);

            var subMal = await _db.HealthCampParticipants
                .AsNoTracking()
                .Where(p => p.HealthCampId == campId
                            && p.PatientId.HasValue
                            && submittedPatientIds.Contains(p.PatientId.Value))
                .Where(p => p.User.Gender != null && p.User.Gender.Name == "Male")
                .CountAsync(ct);

            var fpct = female > 0 ? Math.Min(100, (int)Math.Round(subFem * 100.0 / female)) : 0;
            var mpct = male > 0 ? Math.Min(100, (int)Math.Round(subMal * 100.0 / male)) : 0;

            stationCompletion.Add(new StationCompletionDto { Name = name, Female = fpct, Male = mpct });
        }

        stationCompletion = stationCompletion
            .OrderByDescending(s => Math.Max(s.Female, s.Male))
            .ToList();

        // ── Top Clinical Findings prevalence ──
        // Pull every numeric field response across the camp, classify with VitalThresholds,
        // and count distinct patients who scored Borderline or Abnormal per (Service - Label).
        var rawQ = _db.IntakeFormFieldResponses
            .AsNoTracking()
            .Where(r => r.Response != null
                        && r.Response.HealthCampId == campId
                        && !r.Response.IsDeleted
                        && r.Response.PatientId != Guid.Empty
                        && filteredParticipantIds.Contains(r.Response.PatientId));
        if (filters?.Day.HasValue == true)
        {
            var dayDate2 = camp.StartDate.Date.AddDays(filters.Day.Value - 1);
            rawQ = rawQ.Where(r => r.Response!.CreatedAt.Date == dayDate2);
        }
        var rawResponses = await rawQ
            .Select(r => new
            {
                PatientId = r.Response!.PatientId,
                ServiceId = r.Response!.SubmittedServiceId,
                Label = r.Field != null ? r.Field.Label : null,
                Value = r.Value
            })
            .ToListAsync(ct);

        // Bucket by (label) and count distinct affected patients.
        var labelService = new Dictionary<string, string>(); // label -> service name (first seen)
        var labelAffected = new Dictionary<string, HashSet<Guid>>(); // label -> distinct patients
        var labelEncountered = new Dictionary<string, HashSet<Guid>>(); // label -> all patients tested

        foreach (var resp in rawResponses)
        {
            if (string.IsNullOrWhiteSpace(resp.Label) || string.IsNullOrWhiteSpace(resp.Value)) continue;
            if (!decimal.TryParse(resp.Value, out var num)) continue;

            var label = resp.Label!.Trim();
            if (!labelEncountered.ContainsKey(label)) labelEncountered[label] = new HashSet<Guid>();
            labelEncountered[label].Add(resp.PatientId);

            // Resolve service name once for display
            if (!labelService.ContainsKey(label) && serviceNames.TryGetValue(resp.ServiceId, out var sname))
                labelService[label] = sname;

            var status = VitalThresholds.Classify(label, num);
            if (status != VitalStatus.Normal)
            {
                if (!labelAffected.ContainsKey(label)) labelAffected[label] = new HashSet<Guid>();
                labelAffected[label].Add(resp.PatientId);
            }
        }

        var topFindings = labelAffected
            .Select(kv =>
            {
                var n = kv.Value.Count;
                var tested = labelEncountered.TryGetValue(kv.Key, out var ts) ? ts.Count : 0;
                var pct = tested > 0 ? (int)Math.Round(n * 100.0 / tested) : 0;
                var level = pct >= 70 ? "high" : pct >= 40 ? "med" : "low";
                var displayName = labelService.TryGetValue(kv.Key, out var sn)
                    ? $"{kv.Key} ({sn})"
                    : kv.Key;
                return new TopFindingDto
                {
                    Code = string.Empty,
                    Name = displayName,
                    N = n,
                    Pct = pct,
                    Level = level
                };
            })
            .Where(f => f.N > 0)
            .OrderByDescending(f => f.Pct)
            .ThenByDescending(f => f.N)
            .Take(10)
            .ToList();

                return new CorporateRawDataDto
        {
            CampName = camp.Name,
            ClientName = camp.Organization?.BusinessName ?? string.Empty,
            PackageName = packageName,
            Venue = camp.Location ?? string.Empty,
            StartDate = camp.StartDate,
            EndDate = camp.EndDate,
            ExpectedParticipants = camp.ExpectedParticipants ?? 0,
            TotalAttendees = totalAttendees,
            Female = female,
            Male = male,
            TotalServices = assignmentServiceIds.Count,
            StationCompletion = stationCompletion,
            TopFindings = topFindings,
        };
    }
}
