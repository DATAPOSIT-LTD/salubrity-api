// File: Infrastructure/Repositories/Reporting/FinalCorporateReportRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Reports;
using Salubrity.Application.Interfaces.Repositories.Reporting;
using Salubrity.Application.Services.Reporting.Reports;
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

    // ── Shared helpers ────────────────────────────────────────────────────────

    private static bool TryParseDecimal(string? s, out decimal v)
    {
        v = 0;
        return !string.IsNullOrWhiteSpace(s) &&
               decimal.TryParse(s.Trim(),
                   System.Globalization.NumberStyles.Any,
                   System.Globalization.CultureInfo.InvariantCulture,
                   out v);
    }

    // Snellen acuity worse than 6/24 (decimal < 0.25) or qualitative-blind notation.
    private static bool IsAbnormalSnellen(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var v = value.Trim();
        if (v.Contains('/'))
        {
            var parts = v.Split('/', 2);
            if (decimal.TryParse(parts[0].Trim(), out var num) &&
                decimal.TryParse(parts[1].Trim(), out var den) && den > 0)
                return (num / den) < 0.25m;
        }
        var lv = v.ToLowerInvariant();
        return lv is "cf" or "hm" or "lp" or "nlp";
    }

    private static (int Lo, int Hi) ParseAgeBucket(string label)
    {
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

    // ── GetAgeBucketsAsync ────────────────────────────────────────────────────

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

    // ── GetAgeBucketsFilteredAsync ────────────────────────────────────────────

    public async Task<List<AgeBucketDto>> GetAgeBucketsFilteredAsync(Guid campId, CorporateReportFilters? filters = null, CancellationToken ct = default)
    {
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

    // ── GetEyeVisualHealthAsync ───────────────────────────────────────────────

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
        foreach (var label in new[] { "VA", "Vision" })
        {
            var fieldIds = await _db.IntakeFormFields
                .AsNoTracking()
                .Where(f => !f.IsDeleted && f.Label == label)
                .Where(f => _db.FormSections.Any(s =>
                    !s.IsDeleted && s.Name == eyeSectionName && s.Id == f.SectionId))
                .Select(f => f.Id)
                .ToListAsync(ct);

            if (fieldIds.Count == 0) continue;

            var values = await _db.IntakeFormFieldResponses
                .AsNoTracking()
                .Where(ifr => !ifr.IsDeleted && fieldIds.Contains(ifr.FieldId)
                    && ifr.Value != null && ifr.Value != string.Empty)
                .Where(ifr => _db.IntakeFormResponses.Any(r =>
                    r.Id == ifr.ResponseId && r.HealthCampId == campId && !r.IsDeleted))
                .Select(ifr => ifr.Value!)
                .ToListAsync(ct);

            if (values.Count == 0) continue;

            var mode = values.GroupBy(v => v).OrderByDescending(g => g.Count()).First();
            return (mode.Key, values.Count);
        }

        return ("-", 0);
    }

    // ── GetRecommendationsAsync ───────────────────────────────────────────────

    public async Task<List<FinalRecommendationDto>> GetRecommendationsAsync(Guid campId, CancellationToken ct = default)
    {
        return await _db.HealthAssessmentRecommendations
            .AsNoTracking()
            .Where(r => !r.IsDeleted && _db.HealthAssessments.Any(ha =>
                ha.Id == r.HealthAssessmentId && ha.HealthCampId == campId && !ha.IsDeleted))
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

    // ── GetExcoCategoryCountsAsync (P0 fix: threshold-based, not visitor-count) ──

    public async Task<ExcoCategoryCountsDto> GetExcoCategoryCountsAsync(Guid campId, CorporateReportFilters? filters = null, CancellationToken ct = default)
    {
        // Resolve filter constraints
        Guid? genderId = null;
        if (!string.IsNullOrWhiteSpace(filters?.Gender) && filters.Gender != "Any")
            genderId = await _db.Genders.AsNoTracking()
                .Where(g => g.Name == filters.Gender)
                .Select(g => (Guid?)g.Id)
                .FirstOrDefaultAsync(ct);

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
            var campStart = await _db.HealthCamps.AsNoTracking()
                .Where(c => c.Id == campId)
                .Select(c => (DateTime?)c.StartDate)
                .FirstOrDefaultAsync(ct);
            if (campStart.HasValue)
            {
                dayStart = DateTime.SpecifyKind(campStart.Value.Date.AddDays(filters.Day.Value - 1), DateTimeKind.Utc);
                dayEnd   = DateTime.SpecifyKind(dayStart.Value.AddDays(1), DateTimeKind.Utc);
            }
        }

        // Total filtered attendees
        var attendeesQuery =
            from p in _db.HealthCampParticipants.AsNoTracking()
            join u in _db.Users.AsNoTracking() on p.UserId equals u.Id
            where p.HealthCampId == campId && !p.IsDeleted
            select new { p.UserId, u.GenderId, u.DateOfBirth };
        if (genderId.HasValue)
            attendeesQuery = attendeesQuery.Where(x => x.GenderId == genderId);
        if (minDob.HasValue && maxDob.HasValue)
            attendeesQuery = attendeesQuery.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
        var totalAttendees = await attendeesQuery.Select(x => x.UserId).Distinct().CountAsync(ct);

        // All field responses for this camp (filters applied in DB for date/gender)
        var fieldBaseQ =
            from ifr in _db.IntakeFormFieldResponses.AsNoTracking()
            join f  in _db.IntakeFormFields.AsNoTracking()   on ifr.FieldId    equals f.Id
            join r  in _db.IntakeFormResponses.AsNoTracking() on ifr.ResponseId equals r.Id
            join pat in _db.Patients.AsNoTracking()           on r.PatientId    equals pat.Id
            join u  in _db.Users.AsNoTracking()               on pat.UserId     equals u.Id
            where r.HealthCampId == campId && !r.IsDeleted && !ifr.IsDeleted && !f.IsDeleted
            select new { r.PatientId, FieldLabel = f.Label, ifr.Value, r.CreatedAt, u.GenderId, u.DateOfBirth };

        if (genderId.HasValue)
            fieldBaseQ = fieldBaseQ.Where(x => x.GenderId == genderId);
        if (minDob.HasValue && maxDob.HasValue)
            fieldBaseQ = fieldBaseQ.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
        if (dayStart.HasValue && dayEnd.HasValue)
            fieldBaseQ = fieldBaseQ.Where(x => x.CreatedAt >= dayStart && x.CreatedAt < dayEnd);

        var fieldRows = await fieldBaseQ
            .Select(x => new { x.PatientId, x.FieldLabel, x.Value })
            .ToListAsync(ct);

        // ── Blood pressure: threshold-based (sys ≥ 140 OR dia ≥ 90 = Abnormal) ──
        var bpKws = new[] { "systolic", "diastolic", "blood pressure" };
        var bpGroups = fieldRows
            .Where(f => bpKws.Any(k => (f.FieldLabel ?? string.Empty).ToLowerInvariant().Contains(k)))
            .GroupBy(f => f.PatientId).ToList();

        int bpMeasured = bpGroups.Count;
        int bpAbnormal = bpGroups.Count(grp =>
        {
            // Try combined "120/80" value
            var combined = grp.FirstOrDefault(f =>
            {
                var lbl = (f.FieldLabel ?? string.Empty).ToLowerInvariant();
                return lbl.Contains("blood pressure") && !lbl.Contains("systolic") && !lbl.Contains("diastolic");
            });
            if (combined != null && VitalThresholds.ClassifyBloodPressure(combined.Value) == VitalStatus.Abnormal)
                return true;

            // Separate systolic field
            var sysRow = grp.FirstOrDefault(f => (f.FieldLabel ?? string.Empty).ToLowerInvariant().Contains("systolic"));
            if (sysRow != null && TryParseDecimal(sysRow.Value, out var sv) &&
                VitalThresholds.Classify("blood pressure systolic", sv) == VitalStatus.Abnormal)
                return true;

            // Separate diastolic field
            var diaRow = grp.FirstOrDefault(f => (f.FieldLabel ?? string.Empty).ToLowerInvariant().Contains("diastolic"));
            if (diaRow != null && TryParseDecimal(diaRow.Value, out var dv) &&
                VitalThresholds.Classify("blood pressure diastolic", dv) == VitalStatus.Abnormal)
                return true;

            return false;
        });

        // ── Pre-diabetes: RBS / glucose ≥ 7.8 mmol/L (Borderline or Abnormal) ──
        var rbsKws = new[] { "rbs", "blood sugar", "glucose", "random blood sugar", "fasting blood" };
        var rbsGroups = fieldRows
            .Where(f => rbsKws.Any(k => (f.FieldLabel ?? string.Empty).ToLowerInvariant().Contains(k)))
            .GroupBy(f => f.PatientId).ToList();

        int preDiabetesMeasured = rbsGroups.Count;
        int preDiabetes = rbsGroups.Count(grp =>
            grp.Any(f => TryParseDecimal(f.Value, out var v) &&
                         VitalThresholds.Classify("blood sugar glucose", v) != VitalStatus.Normal));

        // ── Mental health: PHQ-style score ≥ 10 = flagged ──
        var mhKws = new[] { "phq", "mental", "depression", "anxiety", "wellbeing", "well-being", "mood" };
        var mhGroups = fieldRows
            .Where(f => mhKws.Any(k => (f.FieldLabel ?? string.Empty).ToLowerInvariant().Contains(k)))
            .GroupBy(f => f.PatientId).ToList();

        int mhMeasured = mhGroups.Count;
        int mhAbnormal = mhGroups.Count(grp =>
            grp.Any(f => TryParseDecimal(f.Value, out var v) && v >= 10));

        // ── Vision: section-based (Left Eye / Right Eye) + abnormal Snellen detection ──
        var visionQ =
            from ifr in _db.IntakeFormFieldResponses.AsNoTracking()
            join f  in _db.IntakeFormFields.AsNoTracking()    on ifr.FieldId    equals f.Id
            join s  in _db.FormSections.AsNoTracking()        on f.SectionId    equals s.Id
            join r  in _db.IntakeFormResponses.AsNoTracking() on ifr.ResponseId equals r.Id
            join pat in _db.Patients.AsNoTracking()           on r.PatientId    equals pat.Id
            join u  in _db.Users.AsNoTracking()               on pat.UserId     equals u.Id
            where !ifr.IsDeleted && !r.IsDeleted && !s.IsDeleted && !f.IsDeleted
                  && r.HealthCampId == campId
                  && (s.Name == "Left Eye" || s.Name == "Right Eye")
            select new { r.PatientId, ifr.Value, r.CreatedAt, u.GenderId, u.DateOfBirth };

        if (genderId.HasValue) visionQ = visionQ.Where(x => x.GenderId == genderId);
        if (minDob.HasValue && maxDob.HasValue)
            visionQ = visionQ.Where(x => x.DateOfBirth.HasValue && x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
        if (dayStart.HasValue && dayEnd.HasValue)
            visionQ = visionQ.Where(x => x.CreatedAt >= dayStart && x.CreatedAt < dayEnd);

        var visionRows = await visionQ.Select(x => new { x.PatientId, x.Value }).ToListAsync(ct);
        int visionMeasured = visionRows.Select(v => v.PatientId).Distinct().Count();
        int visionAbnormal = visionRows
            .GroupBy(v => v.PatientId)
            .Count(g => g.Any(v => IsAbnormalSnellen(v.Value)));

        // ── CDMP: form-name match (referral/chronic disease management forms) ──
        var pairsQuery =
            from r  in _db.IntakeFormResponses.AsNoTracking()
            join v  in _db.IntakeFormVersions.AsNoTracking() on r.IntakeFormVersionId equals v.Id
            join f  in _db.IntakeForms.AsNoTracking()        on v.IntakeFormId         equals f.Id
            join p  in _db.Patients.AsNoTracking()           on r.PatientId            equals p.Id
            join u  in _db.Users.AsNoTracking()              on p.UserId               equals u.Id
            where r.HealthCampId == campId && !r.IsDeleted
            select new { r.PatientId, FormName = f.Name, ResponseAt = r.CreatedAt, u.GenderId, u.DateOfBirth };

        if (genderId.HasValue)  pairsQuery = pairsQuery.Where(x => x.GenderId == genderId);
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
                .Select(p => p.PatientId).Distinct().Count();
        }

        int cdmp = CountMatching(new[] { "chronic", "referral", "cdmp" });

        // Fallback: if section-based eye exam returned nothing, use form-name match for the measured count
        if (visionMeasured == 0)
            visionMeasured = CountMatching(new[] { "eye", "vision", "optomet" });

        return new ExcoCategoryCountsDto
        {
            TotalAttendees        = totalAttendees,
            Vision                = visionAbnormal,
            VisionMeasured        = visionMeasured,
            Bp                    = bpAbnormal,
            BpMeasured            = bpMeasured,
            PreDiabetes           = preDiabetes,
            PreDiabetesMeasured   = preDiabetesMeasured,
            MentalHealth          = mhAbnormal,
            MentalHealthMeasured  = mhMeasured,
            Cdmp                  = cdmp,
        };
    }

    // ── GetFollowUpCountsAsync ────────────────────────────────────────────────

    public async Task<CampFollowUpCountsDto> GetFollowUpCountsAsync(Guid campId, CancellationToken ct = default)
    {
        var referred = await _db.ServiceReferrals
            .AsNoTracking()
            .Where(r => r.HealthCampId == campId && !r.IsDeleted)
            .Select(r => r.ParticipantId)
            .Distinct()
            .CountAsync(ct);

        var total = await _db.HealthCampParticipants
            .AsNoTracking()
            .Where(p => p.HealthCampId == campId && !p.IsDeleted)
            .CountAsync(ct);

        return new CampFollowUpCountsDto { ReferredCount = referred, TotalAttendees = total };
    }

    // ── GetAbnormalPatientCountAsync ──────────────────────────────────────────

    public async Task<int> GetAbnormalPatientCountAsync(Guid campId, CancellationToken ct = default)
    {
        var rows = await (
            from ifr in _db.IntakeFormFieldResponses.AsNoTracking()
            join f in _db.IntakeFormFields.AsNoTracking()    on ifr.FieldId    equals f.Id
            join r in _db.IntakeFormResponses.AsNoTracking() on ifr.ResponseId equals r.Id
            where r.HealthCampId == campId && !r.IsDeleted && !ifr.IsDeleted && !f.IsDeleted
                  && ifr.Value != null && ifr.Value != string.Empty
            select new { r.PatientId, FieldLabel = f.Label, ifr.Value }
        ).ToListAsync(ct);

        return rows
            .GroupBy(r => r.PatientId)
            .Count(group => group.Any(r =>
                TryParseDecimal(r.Value, out var v) &&
                VitalThresholds.Classify(r.FieldLabel ?? string.Empty, v) == VitalStatus.Abnormal));
    }

    // ── GetLifestyleRiskDistributionAsync ─────────────────────────────────────

    public async Task<List<RiskSliceDto>> GetLifestyleRiskDistributionAsync(Guid campId, CancellationToken ct = default)
    {
        var vitalKws = new[] { "bmi", "body mass", "systolic", "diastolic", "blood pressure", "blood sugar", "glucose", "rbs", "cholesterol" };

        var rows = await (
            from ifr in _db.IntakeFormFieldResponses.AsNoTracking()
            join f in _db.IntakeFormFields.AsNoTracking()    on ifr.FieldId    equals f.Id
            join r in _db.IntakeFormResponses.AsNoTracking() on ifr.ResponseId equals r.Id
            where r.HealthCampId == campId && !r.IsDeleted && !ifr.IsDeleted && !f.IsDeleted
                  && ifr.Value != null && ifr.Value != string.Empty
            select new { r.PatientId, FieldLabel = f.Label, ifr.Value }
        ).ToListAsync(ct);

        var vitalRows = rows
            .Where(r => vitalKws.Any(k => (r.FieldLabel ?? string.Empty).ToLowerInvariant().Contains(k)))
            .ToList();

        if (vitalRows.Count == 0)
            return new List<RiskSliceDto>
            {
                new() { Label = "Low",       Value = 0 },
                new() { Label = "Medium",    Value = 0 },
                new() { Label = "High",      Value = 0 },
                new() { Label = "Very High", Value = 0 },
            };

        var patientScores = vitalRows
            .GroupBy(r => r.PatientId)
            .Select(group =>
            {
                int score = 0;
                foreach (var row in group)
                {
                    if (!TryParseDecimal(row.Value, out var v)) continue;
                    var status = VitalThresholds.Classify(row.FieldLabel ?? string.Empty, v);
                    if (status == VitalStatus.Abnormal)  score += 2;
                    else if (status == VitalStatus.Borderline) score += 1;
                }
                return score;
            })
            .ToList();

        int total  = patientScores.Count;
        int low    = patientScores.Count(s => s == 0);
        int medium = patientScores.Count(s => s >= 1 && s <= 2);
        int high   = patientScores.Count(s => s >= 3 && s <= 4);
        int vHigh  = patientScores.Count(s => s >= 5);

        return new List<RiskSliceDto>
        {
            new() { Label = "Low",       Value = (int)Math.Round(low    * 100.0 / total) },
            new() { Label = "Medium",    Value = (int)Math.Round(medium * 100.0 / total) },
            new() { Label = "High",      Value = (int)Math.Round(high   * 100.0 / total) },
            new() { Label = "Very High", Value = (int)Math.Round(vHigh  * 100.0 / total) },
        };
    }

    // ── GetMentalHealthCountsAsync ────────────────────────────────────────────

    public async Task<CampMentalHealthCountsDto> GetMentalHealthCountsAsync(Guid campId, CancellationToken ct = default)
    {
        var mhKws = new[] { "phq", "mental", "depression", "anxiety", "wellbeing", "well-being", "mood" };

        var rows = await (
            from ifr in _db.IntakeFormFieldResponses.AsNoTracking()
            join f in _db.IntakeFormFields.AsNoTracking()    on ifr.FieldId    equals f.Id
            join s in _db.FormSections.AsNoTracking()        on f.SectionId    equals s.Id
            join r in _db.IntakeFormResponses.AsNoTracking() on ifr.ResponseId equals r.Id
            where r.HealthCampId == campId && !r.IsDeleted && !ifr.IsDeleted && !f.IsDeleted
            select new { r.PatientId, FieldLabel = f.Label, SectionName = s.Name, ifr.Value }
        ).ToListAsync(ct);

        var mhPatients = rows
            .Where(r => mhKws.Any(k => (r.FieldLabel ?? string.Empty).ToLowerInvariant().Contains(k))
                     || mhKws.Any(k => (r.SectionName ?? string.Empty).ToLowerInvariant().Contains(k)))
            .GroupBy(r => r.PatientId)
            .ToList();

        int totalMeasured = mhPatients.Count;
        int lowCount = 0, atRiskCount = 0, goodCount = 0;

        foreach (var group in mhPatients)
        {
            var scores = group
                .Where(r => TryParseDecimal(r.Value, out _))
                .Select(r => { TryParseDecimal(r.Value, out var v); return v; })
                .ToList();

            if (scores.Count == 0) { goodCount++; continue; }

            var maxScore = scores.Max();
            if (maxScore >= 10)      lowCount++;
            else if (maxScore >= 5)  atRiskCount++;
            else                     goodCount++;
        }

        return new CampMentalHealthCountsDto
        {
            GoodCount     = goodCount,
            AtRiskCount   = atRiskCount,
            LowCount      = lowCount,
            TotalMeasured = totalMeasured,
        };
    }

    // ── GetMetabolicNcdRiskAsync ──────────────────────────────────────────────

    public async Task<List<AgeBucketDto>> GetMetabolicNcdRiskAsync(Guid campId, CancellationToken ct = default)
    {
        var vitalKws = new[] { "bmi", "body mass", "systolic", "diastolic", "blood pressure", "blood sugar", "glucose", "rbs", "cholesterol" };

        var rows = await (
            from ifr in _db.IntakeFormFieldResponses.AsNoTracking()
            join f  in _db.IntakeFormFields.AsNoTracking()    on ifr.FieldId    equals f.Id
            join r  in _db.IntakeFormResponses.AsNoTracking() on ifr.ResponseId equals r.Id
            join pat in _db.Patients.AsNoTracking()           on r.PatientId    equals pat.Id
            join u  in _db.Users.AsNoTracking()               on pat.UserId     equals u.Id
            where r.HealthCampId == campId && !r.IsDeleted && !ifr.IsDeleted && !f.IsDeleted
                  && ifr.Value != null && ifr.Value != string.Empty
            select new
            {
                r.PatientId,
                FieldLabel = f.Label,
                ifr.Value,
                GenderName = u.Gender != null ? u.Gender.Name : null,
            }
        ).ToListAsync(ct);

        var vitalRows = rows
            .Where(r => vitalKws.Any(k => (r.FieldLabel ?? string.Empty).ToLowerInvariant().Contains(k)))
            .ToList();

        var patientRisk = vitalRows
            .GroupBy(r => r.PatientId)
            .Select(group =>
            {
                int score = 0;
                string? gender = group.FirstOrDefault()?.GenderName;
                foreach (var row in group)
                {
                    if (!TryParseDecimal(row.Value, out var v)) continue;
                    var status = VitalThresholds.Classify(row.FieldLabel ?? string.Empty, v);
                    if (status == VitalStatus.Abnormal)       score += 2;
                    else if (status == VitalStatus.Borderline) score += 1;
                }
                return new { score, gender };
            })
            .ToList();

        int lowF = patientRisk.Count(p => p.score == 0                && p.gender == "Female");
        int lowM = patientRisk.Count(p => p.score == 0                && p.gender == "Male");
        int modF = patientRisk.Count(p => p.score >= 1 && p.score <= 3 && p.gender == "Female");
        int modM = patientRisk.Count(p => p.score >= 1 && p.score <= 3 && p.gender == "Male");
        int hiF  = patientRisk.Count(p => p.score >= 4                && p.gender == "Female");
        int hiM  = patientRisk.Count(p => p.score >= 4                && p.gender == "Male");

        return new List<AgeBucketDto>
        {
            new() { Label = "Low risk",      Female = lowF, Male = lowM },
            new() { Label = "Moderate risk", Female = modF, Male = modM },
            new() { Label = "High risk",     Female = hiF,  Male = hiM  },
        };
    }
}
