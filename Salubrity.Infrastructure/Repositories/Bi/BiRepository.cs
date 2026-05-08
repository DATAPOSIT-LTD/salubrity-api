using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Bi;
using Salubrity.Application.Interfaces.Repositories.Bi;
using Salubrity.Application.Services.Reporting.Reports;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Bi;

public class BiRepository : IBiRepository
{
    private readonly AppDbContext _db;
    public BiRepository(AppDbContext db) => _db = db;

    // ---- 1. Organizations dimension --------------------------------------
    public async Task<List<BiOrganizationDto>> GetOrganizationsAsync(CancellationToken ct = default)
    {
        return await _db.Organizations
            .AsNoTracking()
            .Where(o => !o.IsDeleted)
            .Select(o => new BiOrganizationDto(
                o.Id,
                o.BusinessName,
                o.Location,
                o.Status != null ? o.Status.Name : null,
                o.CreatedAt
            ))
            .ToListAsync(ct);
    }

    // ---- 2. Camps dimension ----------------------------------------------
    public async Task<List<BiCampDto>> GetCampsAsync(Guid? orgId, int? year, CancellationToken ct = default)
    {
        var q = _db.HealthCamps.AsNoTracking().Where(c => !c.IsDeleted);
        if (orgId.HasValue) q = q.Where(c => c.OrganizationId == orgId.Value);
        if (year.HasValue)
        {
            var start = DateTime.SpecifyKind(new DateTime(year.Value, 1, 1), DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(new DateTime(year.Value + 1, 1, 1), DateTimeKind.Utc);
            q = q.Where(c => c.StartDate >= start && c.StartDate < end);
        }

        var attendance = await _db.IntakeFormResponses
            .AsNoTracking()
            .Where(r => !r.IsDeleted && r.HealthCampId != null)
            .GroupBy(r => r.HealthCampId!.Value)
            .Select(g => new { CampId = g.Key, N = g.Select(x => x.PatientId).Distinct().Count() })
            .ToListAsync(ct);
        var attendanceMap = attendance.ToDictionary(x => x.CampId, x => x.N);

        var camps = await q
            .Select(c => new
            {
                c.Id,
                c.OrganizationId,
                OrgName = c.Organization != null ? c.Organization.BusinessName : "",
                c.Name,
                c.StartDate,
                c.EndDate,
                Status = c.HealthCampStatus != null ? c.HealthCampStatus.Name : null,
                ExpectedParticipants = c.ExpectedParticipants ?? 0,
            })
            .ToListAsync(ct);

        return camps.Select(c => new BiCampDto(
            c.Id, c.OrganizationId, c.OrgName, c.Name, c.StartDate.Year, c.Status,
            c.StartDate, c.EndDate, c.ExpectedParticipants,
            attendanceMap.TryGetValue(c.Id, out var n) ? n : 0
        )).ToList();
    }

    // ---- Shared raw pull --------------------------------------------------
    private record RawRow(
        Guid PatientId,
        Guid CampId,
        Guid OrgId,
        string CampName,
        int Year,
        Guid? GenderId,
        string? GenderName,
        DateTime? Dob,
        string FormName,
        string? Label,
        string? Value
    );

    private async Task<List<RawRow>> PullAsync(Guid? orgId, Guid? campId, int? year, CancellationToken ct)
    {
        var q =
            from r in _db.IntakeFormResponses.AsNoTracking()
            join v in _db.IntakeFormVersions.AsNoTracking() on r.IntakeFormVersionId equals v.Id
            join f in _db.IntakeForms.AsNoTracking() on v.IntakeFormId equals f.Id
            join camp in _db.HealthCamps.AsNoTracking() on r.HealthCampId equals camp.Id
            join p in _db.Patients.AsNoTracking() on r.PatientId equals p.Id
            join u in _db.Users.AsNoTracking() on p.UserId equals u.Id
            join ifr in _db.IntakeFormFieldResponses.AsNoTracking() on r.Id equals ifr.ResponseId
            join field in _db.IntakeFormFields.AsNoTracking() on ifr.FieldId equals field.Id
            where !r.IsDeleted && !ifr.IsDeleted && !field.IsDeleted && !camp.IsDeleted
            select new
            {
                r.PatientId,
                CampId = camp.Id,
                camp.OrganizationId,
                CampName = camp.Name,
                Year = camp.StartDate.Year,
                u.GenderId,
                GenderName = u.Gender != null ? u.Gender.Name : null,
                u.DateOfBirth,
                FormName = f.Name,
                field.Label,
                ifr.Value,
            };

        if (orgId.HasValue) q = q.Where(x => x.OrganizationId == orgId.Value);
        if (campId.HasValue) q = q.Where(x => x.CampId == campId.Value);
        if (year.HasValue)
        {
            var start = DateTime.SpecifyKind(new DateTime(year.Value, 1, 1), DateTimeKind.Utc);
            var end = DateTime.SpecifyKind(new DateTime(year.Value + 1, 1, 1), DateTimeKind.Utc);
            q = q.Where(x => x.Year == year.Value);
        }

        var data = await q.ToListAsync(ct);
        return data.Select(x => new RawRow(
            x.PatientId, x.CampId, x.OrganizationId, x.CampName, x.Year,
            x.GenderId, x.GenderName, x.DateOfBirth, x.FormName, x.Label, x.Value
        )).ToList();
    }

    // ---- Helpers ----------------------------------------------------------
    private static decimal? ParseDec(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;
        return decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d) ? d : (decimal?)null;
    }

    private static (int? sys, int? dia) ParseBp(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return (null, null);
        var parts = s.Split('/', 2);
        if (parts.Length != 2) return (null, null);
        var sys = int.TryParse(parts[0].Trim(), out var a) ? a : (int?)null;
        var dia = int.TryParse(parts[1].Trim(), out var b) ? b : (int?)null;
        return (sys, dia);
    }

    private static int? Age(DateTime? dob, int year)
    {
        if (dob == null) return null;
        var age = year - dob.Value.Year;
        return age >= 0 && age <= 130 ? age : null;
    }

    private static bool LabelContains(string? label, params string[] needles)
    {
        if (string.IsNullOrWhiteSpace(label)) return false;
        var l = label.ToLowerInvariant();
        return needles.Any(n => l.Contains(n));
    }

    private static bool FormContains(string? form, params string[] needles)
    {
        if (string.IsNullOrWhiteSpace(form)) return false;
        var f = form.ToLowerInvariant();
        return needles.Any(n => f.Contains(n));
    }

    // ---- 3. Patient wide rows --------------------------------------------
    public async Task<List<BiPatientRowDto>> GetPatientRowsAsync(Guid? orgId, Guid? campId, int? year, CancellationToken ct = default)
    {
        var raw = await PullAsync(orgId, campId, year, ct);

        return raw
            .GroupBy(r => new { r.PatientId, r.CampId })
            .Select(g =>
            {
                var any = g.First();

                decimal? bmi = null;
                int? bpSys = null, bpDia = null;
                decimal? rbs = null, chol = null, hba1c = null, creat = null, ggt = null;
                string? alcohol = null;
                int? lifestyle = null;

                bool mental = false, visual = false, joint = false, ear = false, nose = false,
                     skin = false, breast = false, mssBack = false;

                foreach (var row in g)
                {
                    if (LabelContains(row.Label, "bmi", "body mass") && bmi == null) bmi = ParseDec(row.Value);
                    else if (LabelContains(row.Label, "blood pressure") && (bpSys == null || bpDia == null))
                    {
                        var (s, d) = ParseBp(row.Value);
                        bpSys ??= s; bpDia ??= d;
                    }
                    else if (LabelContains(row.Label, "random blood sugar", "rbs") && rbs == null) rbs = ParseDec(row.Value);
                    else if (LabelContains(row.Label, "cholesterol") && chol == null) chol = ParseDec(row.Value);
                    else if (LabelContains(row.Label, "hba1c") && hba1c == null) hba1c = ParseDec(row.Value);
                    else if (LabelContains(row.Label, "creatinine") && creat == null) creat = ParseDec(row.Value);
                    else if (LabelContains(row.Label, "ggt", "gamma glutamyl") && ggt == null) ggt = ParseDec(row.Value);
                    else if (LabelContains(row.Label, "alcohol") && alcohol == null) alcohol = row.Value;
                    else if (LabelContains(row.Label, "lifestyle") && lifestyle == null) lifestyle = (int?)ParseDec(row.Value);

                    if (FormContains(row.FormName, "mental")) mental = true;
                    if (FormContains(row.FormName, "visual", "eye")) visual = true;
                    if (FormContains(row.FormName, "physiotherap", "joint", "mss")) joint = true;
                    if (FormContains(row.FormName, "ent", "ear")) ear = true;
                    if (FormContains(row.FormName, "ent", "nose")) nose = true;
                    if (FormContains(row.FormName, "skin", "dermat")) skin = true;
                    if (FormContains(row.FormName, "well woman", "breast")) breast = true;
                    if (FormContains(row.FormName, "mss", "back", "physiotherap")) mssBack = true;
                }

                return new BiPatientRowDto(
                    g.Key.PatientId,
                    g.Key.CampId,
                    any.OrgId,
                    any.CampName,
                    any.Year,
                    null, // Department not modeled on User
                    Age(any.Dob, any.Year),
                    any.GenderName,
                    bmi, bpSys, bpDia, rbs, chol, hba1c, creat, ggt,
                    alcohol, lifestyle,
                    mental, visual, joint, ear, nose, skin, breast, mssBack
                );
            })
            .ToList();
    }

    // ---- 4. Findings (long format) ---------------------------------------
    public async Task<List<BiFindingDto>> GetFindingsAsync(Guid? orgId, Guid? campId, int? year, CancellationToken ct = default)
    {
        var raw = await PullAsync(orgId, campId, year, ct);
        var findings = new List<BiFindingDto>();

        foreach (var r in raw)
        {
            if (string.IsNullOrWhiteSpace(r.Label) || string.IsNullOrWhiteSpace(r.Value)) continue;
            if (!decimal.TryParse(r.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var num)) continue;

            var status = VitalThresholds.Classify(r.Label!, num);
            if (status == VitalStatus.Normal) continue;

            findings.Add(new BiFindingDto(
                r.PatientId,
                r.CampId,
                r.FormName,
                null,
                r.Label!,
                status.ToString()
            ));
        }
        return findings;
    }

    // ---- 5. Lab results (long format) ------------------------------------
    public async Task<List<BiLabResultDto>> GetLabResultsAsync(Guid? orgId, Guid? campId, int? year, CancellationToken ct = default)
    {
        var raw = await PullAsync(orgId, campId, year, ct);

        bool IsLab(string form)
        {
            var f = (form ?? "").ToLowerInvariant();
            return f.Contains("lab test") || f.Contains("hemogram") || f.Contains("liver function")
                || f.Contains("random blood sugar") || f.Contains("lft");
        }

        return raw
            .Where(r => IsLab(r.FormName) && !string.IsNullOrWhiteSpace(r.Label))
            .Select(r => new BiLabResultDto(
                r.PatientId,
                r.CampId,
                r.Label!,
                r.Label!,
                r.Value,
                null,
                null,
                null
            ))
            .ToList();
    }
}
