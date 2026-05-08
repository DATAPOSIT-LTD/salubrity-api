// File: Infrastructure/Services/AdminDashboard/AdminDashboardOverviewService.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.AdminDashboard;
using Salubrity.Application.Interfaces.Services.AdminDashboard;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Services.AdminDashboard;

public sealed class AdminDashboardOverviewService : IAdminDashboardOverviewService
{
    private readonly AppDbContext _db;
    public AdminDashboardOverviewService(AppDbContext db) => _db = db;

    public async Task<AdminDashboardOverviewDto> GetOverviewAsync(int? year = null, CancellationToken ct = default)
    {
        var nowUtc = DateTime.UtcNow;
        var todayUtc = DateTime.SpecifyKind(nowUtc.Date, DateTimeKind.Utc);
        var tomorrowUtc = todayUtc.AddDays(1);
        var ytdYear = year.HasValue && year.Value >= 2000 && year.Value <= nowUtc.Year + 5
            ? year.Value
            : nowUtc.Year;
        var yearStart = DateTime.SpecifyKind(new DateTime(ytdYear, 1, 1), DateTimeKind.Utc);
        // Cap end of window at year-end OR now (so a past year shows the full year, current year shows YTD)
        var yearEndExclusive = ytdYear < nowUtc.Year
            ? DateTime.SpecifyKind(new DateTime(ytdYear + 1, 1, 1), DateTimeKind.Utc)
            : nowUtc;

        // ----- ONGOING -----
        var ongoingCampRaw = await _db.HealthCamps
            .AsNoTracking()
            .Include(c => c.Organization)
            .Where(c => !c.IsDeleted
                && c.StartDate.Date <= todayUtc
                && (c.EndDate == null || c.EndDate.Value.Date >= todayUtc))
            .OrderBy(c => c.StartDate)
            .Select(c => new
            {
                c.Id, c.Name, c.Location, c.StartDate, c.EndDate, c.ExpectedParticipants,
                ClientName = c.Organization != null ? c.Organization.BusinessName : string.Empty,
            })
            .FirstOrDefaultAsync(ct);

        OngoingCampSummaryDto? ongoing = null;
        if (ongoingCampRaw != null)
        {
            var totalAttendees = await _db.HealthCampParticipants.AsNoTracking()
                .Where(p => p.HealthCampId == ongoingCampRaw.Id && !p.IsDeleted)
                .Select(p => p.UserId).Distinct().CountAsync(ct);

            var formsToday = await _db.IntakeFormResponses.AsNoTracking()
                .Where(r => r.HealthCampId == ongoingCampRaw.Id && !r.IsDeleted
                    && r.CreatedAt >= todayUtc && r.CreatedAt < tomorrowUtc)
                .CountAsync(ct);
            var formsTotal = await _db.IntakeFormResponses.AsNoTracking()
                .Where(r => r.HealthCampId == ongoingCampRaw.Id && !r.IsDeleted)
                .CountAsync(ct);
            var activeStations = await _db.IntakeFormResponses.AsNoTracking()
                .Where(r => r.HealthCampId == ongoingCampRaw.Id && !r.IsDeleted
                    && r.CreatedAt >= todayUtc && r.CreatedAt < tomorrowUtc)
                .Select(r => r.ResolvedServiceId).Distinct().CountAsync(ct);
            var subcontractorCount = await _db.SubcontractorHealthCampAssignments.AsNoTracking()
                .Where(a => a.HealthCampId == ongoingCampRaw.Id && !a.IsDeleted)
                .Select(a => a.SubcontractorId).Distinct().CountAsync(ct);

            var startDay = ongoingCampRaw.StartDate.Date;
            var endDay = (ongoingCampRaw.EndDate ?? ongoingCampRaw.StartDate).Date;
            var dayNumber = (int)(todayUtc - startDay).TotalDays + 1;
            var daysTotal = Math.Max(1, (int)(endDay - startDay).TotalDays + 1);
            var expected = ongoingCampRaw.ExpectedParticipants ?? 0;
            var pRate = expected > 0 ? Math.Min(100, (int)Math.Round(totalAttendees * 100.0 / expected)) : 0;

            ongoing = new OngoingCampSummaryDto
            {
                CampId = ongoingCampRaw.Id,
                Name = ongoingCampRaw.Name,
                ClientName = ongoingCampRaw.ClientName,
                Venue = ongoingCampRaw.Location ?? string.Empty,
                StartDate = ongoingCampRaw.StartDate,
                EndDate = ongoingCampRaw.EndDate,
                DayNumber = Math.Max(1, Math.Min(daysTotal, dayNumber)),
                DaysTotal = daysTotal,
                ExpectedAttendees = expected,
                TotalAttendees = totalAttendees,
                ParticipationRate = pRate,
                FormsSubmittedToday = formsToday,
                FormsSubmittedTotal = formsTotal,
                ActiveStationsToday = activeStations,
                RegisteredSubcontractors = subcontractorCount,
            };
        }

        // ----- NEXT UPCOMING -----
        var upcomingRaw = await _db.HealthCamps.AsNoTracking()
            .Include(c => c.Organization)
            .Where(c => !c.IsDeleted && c.StartDate.Date > todayUtc)
            .OrderBy(c => c.StartDate)
            .Select(c => new
            {
                c.Id, c.Name, c.StartDate, c.ExpectedParticipants,
                ClientName = c.Organization != null ? c.Organization.BusinessName : string.Empty,
            })
            .FirstOrDefaultAsync(ct);
        UpcomingCampSummaryDto? upcoming = null;
        if (upcomingRaw != null)
        {
            upcoming = new UpcomingCampSummaryDto
            {
                CampId = upcomingRaw.Id,
                Name = upcomingRaw.Name,
                ClientName = upcomingRaw.ClientName,
                StartDate = upcomingRaw.StartDate,
                DaysUntil = Math.Max(0, (int)(upcomingRaw.StartDate.Date - todayUtc).TotalDays),
                ExpectedAttendees = upcomingRaw.ExpectedParticipants ?? 0,
            };
        }

        // ----- LAST COMPLETED -----
        var lastRaw = await _db.HealthCamps.AsNoTracking()
            .Include(c => c.Organization)
            .Where(c => !c.IsDeleted && c.EndDate != null && c.EndDate.Value.Date < todayUtc)
            .OrderByDescending(c => c.EndDate)
            .Select(c => new
            {
                c.Id, c.Name, c.EndDate, c.ExpectedParticipants,
                ClientName = c.Organization != null ? c.Organization.BusinessName : string.Empty,
            })
            .FirstOrDefaultAsync(ct);
        LastCompletedCampSummaryDto? last = null;
        if (lastRaw != null)
        {
            var lastAttendees = await _db.HealthCampParticipants.AsNoTracking()
                .Where(p => p.HealthCampId == lastRaw.Id && !p.IsDeleted)
                .Select(p => p.UserId).Distinct().CountAsync(ct);
            var lastExpected = lastRaw.ExpectedParticipants ?? 0;
            var lastRate = lastExpected > 0 ? Math.Min(100, (int)Math.Round(lastAttendees * 100.0 / lastExpected)) : 0;
            last = new LastCompletedCampSummaryDto
            {
                CampId = lastRaw.Id,
                Name = lastRaw.Name,
                ClientName = lastRaw.ClientName,
                EndDate = lastRaw.EndDate,
                TotalAttendees = lastAttendees,
                ExpectedAttendees = lastExpected,
                ParticipationRate = lastRate,
            };
        }

        // ----- YTD TOTALS (selected year) -----
        var ytdCamps = await _db.HealthCamps.AsNoTracking()
            .Where(c => !c.IsDeleted && c.StartDate >= yearStart && c.StartDate < yearEndExclusive)
            .CountAsync(ct);
        var ytdPatients = await _db.IntakeFormResponses.AsNoTracking()
            .Where(r => !r.IsDeleted && r.CreatedAt >= yearStart && r.CreatedAt < yearEndExclusive)
            .Select(r => r.PatientId).Distinct().CountAsync(ct);
        var ytdOrgs = await _db.HealthCamps.AsNoTracking()
            .Where(c => !c.IsDeleted && c.StartDate >= yearStart && c.StartDate < yearEndExclusive)
            .Select(c => c.OrganizationId).Distinct().CountAsync(ct);

        // ----- PENDING FINAL REPORT PUBLISHES -----
        var pendingRaw = await _db.HealthCamps.AsNoTracking()
            .Include(c => c.Organization)
            .Where(c => !c.IsDeleted
                && c.EndDate != null && c.EndDate.Value.Date < todayUtc
                && c.FinalReportsPublishedAt == null)
            .OrderByDescending(c => c.EndDate)
            .Take(10)
            .Select(c => new
            {
                c.Id, c.Name, c.EndDate,
                ClientName = c.Organization != null ? c.Organization.BusinessName : string.Empty,
            })
            .ToListAsync(ct);
        var pending = pendingRaw.Select(p => new PendingFinalReportDto
        {
            CampId = p.Id,
            Name = p.Name,
            ClientName = p.ClientName,
            EndDate = p.EndDate,
            DaysSinceCompletion = p.EndDate.HasValue ? Math.Max(0, (int)(todayUtc - p.EndDate.Value.Date).TotalDays) : 0,
        }).ToList();

        return new AdminDashboardOverviewDto
        {
            OngoingCamp = ongoing,
            NextUpcomingCamp = upcoming,
            LastCompletedCamp = last,
            YtdTotals = new YtdTotalsDto
            {
                CampsRun = ytdCamps,
                PatientsScreened = ytdPatients,
                OrganizationsServed = ytdOrgs,
                Year = ytdYear,
            },
            PendingFinalReportPublishes = pending,
        };
    }
}
