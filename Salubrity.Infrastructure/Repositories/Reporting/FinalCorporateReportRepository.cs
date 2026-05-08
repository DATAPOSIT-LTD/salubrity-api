// File: Infrastructure/Repositories/Reporting/FinalCorporateReportRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Reports;
using Salubrity.Application.Interfaces.Repositories.Reporting;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Reporting;

public sealed class FinalCorporateReportRepository : IFinalCorporateReportRepository
{
    private readonly AppDbContext _db;
    public FinalCorporateReportRepository(AppDbContext db) => _db = db;

    private static readonly (string Label, int Min, int Max)[] AgeBands = new[]
    {
        ("18-25", 18, 25),
        ("26-35", 26, 35),
        ("36-45", 36, 45),
        ("46-55", 46, 55),
        ("56-65", 56, 65),
        ("66+",   66, 200),
    };

    public async Task<List<AgeBucketDto>> GetAgeBucketsAsync(Guid campId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;
        var rows = await _db.HealthCampParticipants
            .AsNoTracking()
            .Where(p => p.HealthCampId == campId && !p.IsDeleted)
            .Select(p => new
            {
                Dob = p.User.DateOfBirth,
                GenderName = p.User.Gender != null ? p.User.Gender.Name : null,
            })
            .ToListAsync(ct);

        var withAge = rows
            .Where(r => r.Dob.HasValue)
            .Select(r =>
            {
                var d = r.Dob!.Value.Date;
                var age = today.Year - d.Year;
                if (d > today.AddYears(-age)) age--;
                return new { Age = age, Gender = r.GenderName };
            })
            .Where(x => x.Age >= 18 && x.Age <= 200)
            .ToList();

        return AgeBands.Select(b => new AgeBucketDto
        {
            Label = b.Label,
            Female = withAge.Count(x => x.Age >= b.Min && x.Age <= b.Max && x.Gender == "Female"),
            Male   = withAge.Count(x => x.Age >= b.Min && x.Age <= b.Max && x.Gender == "Male"),
        }).ToList();
    }

    public async Task<ExcoCategoryCountsDto> GetExcoCategoryCountsAsync(Guid campId, CorporateReportFilters? filters = null, CancellationToken ct = default)
    {
        // Resolve gender/age/day filter constraints once
        Guid? genderId = null;
        if (!string.IsNullOrWhiteSpace(filters?.Gender) && filters.Gender != "Any")
            genderId = await _db.Genders.AsNoTracking().Where(g => g.Name == filters.Gender).Select(g => (Guid?)g.Id).FirstOrDefaultAsync(ct);

        DateTime? minDob = null, maxDob = null;
        if (!string.IsNullOrWhiteSpace(filters?.AgeBucket) && filters.AgeBucket != "Any")
        {
            var (lo, hi) = ParseAgeBucket(filters.AgeBucket);
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            maxDob = DateTime.SpecifyKind(today.AddYears(-lo), DateTimeKind.Utc);
            minDob = DateTime.SpecifyKind(today.AddYears(-hi - 1).AddDays(1), DateTimeKind.Utc);
        }

        DateTime? dayStart = null, dayEnd = null;
        if (filters?.Day.HasValue == true)
        {
            var campStart = await _db.HealthCamps.AsNoTracking().Where(c => c.Id == campId)
                .Select(c => (DateTime?)c.StartDate).FirstOrDefaultAsync(ct);
            if (campStart.HasValue)
            {
                dayStart = DateTime.SpecifyKind(campStart.Value.Date.AddDays(filters.Day.Value - 1), DateTimeKind.Utc);
                dayEnd = DateTime.SpecifyKind(dayStart.Value.AddDays(1), DateTimeKind.Utc);
            }
        }

        // Filtered total attendees = distinct participants matching filters
        var attendeesQuery =
            from p in _db.HealthCampParticipants.AsNoTracking()
            join u in _db.Users.AsNoTracking() on p.UserId equals u.Id
            where p.HealthCampId == campId && !p.IsDeleted
            select new { p.UserId, u.GenderId, u.DateOfBirth };

        if (genderId.HasValue) attendeesQuery = attendeesQuery.Where(x => x.GenderId == genderId);
        if (minDob.HasValue && maxDob.HasValue)
            attendeesQuery = attendeesQuery.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);

        var totalAttendees = await attendeesQuery.Select(x => x.UserId).Distinct().CountAsync(ct);

        // Filtered (PatientId, FormName) pairs
        var pairsQuery =
            from r in _db.IntakeFormResponses.AsNoTracking()
            join v in _db.IntakeFormVersions.AsNoTracking() on r.IntakeFormVersionId equals v.Id
            join f in _db.IntakeForms.AsNoTracking() on v.IntakeFormId equals f.Id
            join p in _db.Patients.AsNoTracking() on r.PatientId equals p.Id
            join u in _db.Users.AsNoTracking() on p.UserId equals u.Id
            where r.HealthCampId == campId && !r.IsDeleted
            select new { r.PatientId, FormName = f.Name, ResponseAt = r.CreatedAt, u.GenderId, u.DateOfBirth };

        if (genderId.HasValue) pairsQuery = pairsQuery.Where(x => x.GenderId == genderId);
        if (minDob.HasValue && maxDob.HasValue)
            pairsQuery = pairsQuery.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
        if (dayStart.HasValue && dayEnd.HasValue)
            pairsQuery = pairsQuery.Where(x => x.ResponseAt >= dayStart && x.ResponseAt < dayEnd);

        var pairs = await pairsQuery.Select(x => new { x.PatientId, x.FormName }).ToListAsync(ct);

        int CountMatching(string[] patterns)
        {
            var lc = patterns.Select(p => p.ToLowerInvariant()).ToArray();
            return pairs
                .Where(p => lc.Any(pat => (p.FormName ?? string.Empty).ToLowerInvariant().Contains(pat)))
                .Select(p => p.PatientId)
                .Distinct()
                .Count();
        }

        // Vision: prefer counting patients who actually filled a Left Eye / Right Eye section field;
        // fall back to form-name match ("eye" / "optomet" / "vision"). Apply same filters.
        var visionQ =
            from ifr in _db.IntakeFormFieldResponses.AsNoTracking()
            join f in _db.IntakeFormFields.AsNoTracking() on ifr.FieldId equals f.Id
            join s in _db.FormSections.AsNoTracking() on f.SectionId equals s.Id
            join r in _db.IntakeFormResponses.AsNoTracking() on ifr.ResponseId equals r.Id
            join pat in _db.Patients.AsNoTracking() on r.PatientId equals pat.Id
            join u in _db.Users.AsNoTracking() on pat.UserId equals u.Id
            where !ifr.IsDeleted && !r.IsDeleted && !s.IsDeleted && !f.IsDeleted
                  && r.HealthCampId == campId
                  && (s.Name == "Left Eye" || s.Name == "Right Eye")
            select new { r.PatientId, r.CreatedAt, u.GenderId, u.DateOfBirth };
        if (genderId.HasValue) visionQ = visionQ.Where(x => x.GenderId == genderId);
        if (minDob.HasValue && maxDob.HasValue)
            visionQ = visionQ.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
        if (dayStart.HasValue && dayEnd.HasValue)
            visionQ = visionQ.Where(x => x.CreatedAt >= dayStart && x.CreatedAt < dayEnd);
        var visionFromSections = await visionQ.Select(x => x.PatientId).Distinct().CountAsync(ct);

        var vision = visionFromSections > 0 ? visionFromSections : CountMatching(new[] { "eye", "vision", "optomet" });
        var bp = CountMatching(new[] { "triage", "physical exam" });
        var preDiabetes = CountMatching(new[] { "triage service diagnosis", "glucose", "rbs", "blood sugar", "random blood sugar" });
        var mentalHealth = CountMatching(new[] { "mental" });
        var cdmp = CountMatching(new[] { "chronic", "referral", "cdmp" });

        return new ExcoCategoryCountsDto
        {
            TotalAttendees = totalAttendees,
            Vision = vision,
            Bp = bp,
            PreDiabetes = preDiabetes,
            MentalHealth = mentalHealth,
            Cdmp = cdmp,
        };
    }

    public async Task<List<AgeBucketDto>> GetAgeBucketsFilteredAsync(Guid campId, CorporateReportFilters? filters = null, CancellationToken ct = default)
    {
        // For now filtering only narrows to participants matching gender/age — day filter not meaningful for demographics
        Guid? genderId = null;
        if (!string.IsNullOrWhiteSpace(filters?.Gender) && filters.Gender != "Any")
            genderId = await _db.Genders.AsNoTracking().Where(g => g.Name == filters.Gender).Select(g => (Guid?)g.Id).FirstOrDefaultAsync(ct);
        DateTime? minDob = null, maxDob = null;
        if (!string.IsNullOrWhiteSpace(filters?.AgeBucket) && filters.AgeBucket != "Any")
        {
            var (lo, hi) = ParseAgeBucket(filters.AgeBucket);
            var today = DateTime.SpecifyKind(DateTime.UtcNow.Date, DateTimeKind.Utc);
            maxDob = DateTime.SpecifyKind(today.AddYears(-lo), DateTimeKind.Utc);
            minDob = DateTime.SpecifyKind(today.AddYears(-hi - 1).AddDays(1), DateTimeKind.Utc);
        }

        var today2 = DateTime.UtcNow.Date;
        var q =
            from p in _db.HealthCampParticipants.AsNoTracking()
            join u in _db.Users.AsNoTracking() on p.UserId equals u.Id
            where p.HealthCampId == campId && !p.IsDeleted
            select new { Dob = u.DateOfBirth, GenderName = u.Gender != null ? u.Gender.Name : null, u.GenderId };
        if (genderId.HasValue) q = q.Where(x => x.GenderId == genderId);
        if (minDob.HasValue && maxDob.HasValue)
            q = q.Where(x => x.Dob.HasValue && x.Dob >= minDob && x.Dob <= maxDob);
        var rows = await q.ToListAsync(ct);

        var withAge = rows.Where(r => r.Dob.HasValue).Select(r =>
        {
            var d = r.Dob!.Value.Date;
            var age = today2.Year - d.Year;
            if (d > today2.AddYears(-age)) age--;
            return new { Age = age, Gender = r.GenderName };
        }).Where(x => x.Age >= 18 && x.Age <= 200).ToList();

        return AgeBands.Select(b => new AgeBucketDto
        {
            Label = b.Label,
            Female = withAge.Count(x => x.Age >= b.Min && x.Age <= b.Max && x.Gender == "Female"),
            Male   = withAge.Count(x => x.Age >= b.Min && x.Age <= b.Max && x.Gender == "Male"),
        }).ToList();
    }

    private static (int Lo, int Hi) ParseAgeBucket(string label)
    {
        // Accepts "18-30", "31-45", "46-60", "60+", "66+" etc.
        var trimmed = (label ?? string.Empty).Trim();
        if (trimmed.EndsWith("+"))
        {
            if (int.TryParse(trimmed.TrimEnd('+'), out var lo)) return (lo, 200);
            return (0, 200);
        }
        var parts = trimmed.Split('-');
        if (parts.Length == 2 && int.TryParse(parts[0], out var l) && int.TryParse(parts[1], out var h))
            return (l, h);
        return (0, 200);
    }

    public async Task<EyeVisualAggregateDto> GetEyeVisualHealthAsync(Guid campId, CancellationToken ct = default)
    {
        var (leftMode, leftCount) = await EyeModeAsync(campId, "Left Eye", ct);
        var (rightMode, rightCount) = await EyeModeAsync(campId, "Right Eye", ct);
        return new EyeVisualAggregateDto
        {
            LeftEyeAcuity = leftMode,
            RightEyeAcuity = rightMode,
            LeftEyeCount = leftCount,
            RightEyeCount = rightCount,
        };
    }

    private async Task<(string Mode, int Count)> EyeModeAsync(Guid campId, string eyeSectionName, CancellationToken ct)
    {
        // Try VA (visual acuity) first, fall back to Vision
        foreach (var label in new[] { "VA", "Vision" })
        {
            var fieldIds = await _db.IntakeFormFields
                .AsNoTracking()
                .Where(f => !f.IsDeleted && f.Label == label)
                .Where(f => _db.FormSections.Any(s =>
                    !s.IsDeleted && s.Name == eyeSectionName &&
                    s.Id == f.SectionId))
                .Select(f => f.Id)
                .ToListAsync(ct);

            if (fieldIds.Count == 0) continue;

            var values = await _db.IntakeFormFieldResponses
                .AsNoTracking()
                .Where(ifr => !ifr.IsDeleted && fieldIds.Contains(ifr.FieldId)
                    && ifr.Value != null && ifr.Value != string.Empty)
                .Where(ifr => _db.IntakeFormResponses.Any(r => r.Id == ifr.ResponseId
                    && r.HealthCampId == campId && !r.IsDeleted))
                .Select(ifr => ifr.Value!)
                .ToListAsync(ct);

            if (values.Count == 0) continue;

            var mode = values.GroupBy(v => v).OrderByDescending(g => g.Count()).First();
            return (mode.Key, values.Count);
        }

        return ("-", 0);
    }

    public async Task<List<FinalRecommendationDto>> GetRecommendationsAsync(Guid campId, CancellationToken ct = default)
    {
        return await _db.HealthAssessmentRecommendations
            .AsNoTracking()
            .Where(r => !r.IsDeleted && _db.HealthAssessments.Any(ha => ha.Id == r.HealthAssessmentId && ha.HealthCampId == campId && !ha.IsDeleted))
            .OrderByDescending(r => r.Priority)
            .Select(r => new FinalRecommendationDto
            {
                Id = r.Id,
                Title = r.Title,
                Recommendation = r.Description ?? string.Empty,
                ImplementationNote = string.Empty,
                Priority = r.Priority.HasValue
                    ? (r.Priority.Value >= 3 ? "High" : r.Priority.Value == 2 ? "Medium" : "Low")
                    : "Medium",
                IsRiskBased = true,
            })
            .ToListAsync(ct);
    }
}
