using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Infrastructure.Repositories.HealthCamps;

public class CampStatisticsRepository : ICampStatisticsRepository
{
    private readonly AppDbContext _context;

    public CampStatisticsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CampStatsResponseDto> GetCampStatsAsync(Guid campId, CancellationToken ct)
    {
        // =========================================================
        // 1. CAMP (AUTHORITATIVE)
        // =========================================================
        var camp = await _context.HealthCamps
            .AsNoTracking()
            .Where(c => c.Id == campId && !c.IsDeleted)
            .Select(c => new
            {
                c.Id,
                c.Name,
                c.IsActive
            })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("HealthCamp", campId.ToString());

        // =========================================================
        // 2. PARTICIPANTS
        // =========================================================
        var participantsCount = await _context.HealthCampParticipants
            .CountAsync(p => p.HealthCampId == campId && !p.IsDeleted, ct);

        // =========================================================
        // 3. STAFF / SUBCONTRACTORS (OPERATIONAL ASSIGNMENTS)
        // =========================================================
        var staffCount = await _context.SubcontractorHealthCampAssignments
            .Where(a =>
                a.HealthCampId == campId &&
                !a.IsDeleted)
            .Select(a => a.SubcontractorId)
            .Distinct()
            .CountAsync(ct);

        // =========================================================
        // 4. PACKAGES
        // =========================================================
        var totalPackages = await _context.HealthCampPackages
            .CountAsync(p => p.HealthCampId == campId && !p.IsDeleted, ct);

        var assignedPackages = await _context.HealthCampParticipantPackages
            .Where(p =>
                p.IsActive &&
                !p.IsDeleted &&
                _context.HealthCampParticipants.Any(hp =>
                    hp.Id == p.ParticipantId &&
                    hp.HealthCampId == campId))
            .CountAsync(ct);

        // =========================================================
        // 5. SERVICES (DESIGN-TIME)
        // =========================================================
        var totalServices = await _context.HealthCampServiceAssignments
            .CountAsync(s =>
                s.HealthCampId == campId &&
                !s.IsDeleted,
                ct);

        // =========================================================
        // 6. SERVICES SERVED (OPERATIONAL TRUTH — FIXED)
        // =========================================================
        var servedServices = await (
            from r in _context.IntakeFormResponses
            join p in _context.Patients
                on r.PatientId equals p.Id
            join hcp in _context.HealthCampParticipants
                on p.UserId equals hcp.UserId
            where
                hcp.HealthCampId == campId &&
                !hcp.IsDeleted &&
                !r.IsDeleted
            select new
            {
                r.PatientId,
                r.ResolvedServiceId
            }
        )
        .Distinct()
        .CountAsync(ct);

        // =========================================================
        // 7. RETURN DTO (DONUT-READY)
        // =========================================================
        return new CampStatsResponseDto
        {
            CampId = camp.Id,
            CampName = camp.Name,
            IsActive = camp.IsActive,

            Counts = new CampStatsCountsDto
            {
                Participants = participantsCount,
                Staff = staffCount,
            },

            Packages = new CampStatsPackageDto
            {
                TotalPackages = totalPackages,
                AssignedPackages = assignedPackages,
                UnassignedParticipants = Math.Max(
                    participantsCount - assignedPackages, 0)
            },

            Services = new CampStatsServiceDto
            {
                TotalServices = totalServices,
                ServedServices = servedServices,
                PendingServices = Math.Max(
                    (totalServices * participantsCount) - servedServices, 0)
            }
        };
    }

    public async Task<CampSaStatusDto> GetCampSaStatusAsync(Guid campId, CancellationToken ct)
    {
        var camp = await _context.HealthCamps
            .AsNoTracking()
            .Where(c => c.Id == campId && !c.IsDeleted)
            .Select(c => new { c.RequiresSelfAssessment })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("HealthCamp", campId.ToString());

        var saId0 = Guid.Parse("bcad133b-9b8a-47e5-8551-e86069cdd80d");
        var saId1 = Guid.Parse("4ab8a5f2-8f8d-4c49-97ec-cdaff0c51843");
        var saId2 = Guid.Parse("bd5e98e0-7389-4e25-9712-84a7bfe68f63");

        var participants = await _context.HealthCampParticipants
            .AsNoTracking()
            .Where(p => p.HealthCampId == campId && !p.IsDeleted)
            .Select(p => new
            {
                p.UserId,
                p.User.FullName,
                Email = p.User.Email,
                Phone = p.User.Phone,
                HasSa =
                    _context.HealthAssessmentFormResponses.Any(r => r.CreatedBy == p.UserId && !r.IsDeleted && r.FormTypeId == saId0) &&
                    _context.HealthAssessmentFormResponses.Any(r => r.CreatedBy == p.UserId && !r.IsDeleted && r.FormTypeId == saId1) &&
                    _context.HealthAssessmentFormResponses.Any(r => r.CreatedBy == p.UserId && !r.IsDeleted && r.FormTypeId == saId2)
            })
            .ToListAsync(ct);

        var completed = participants
            .Where(p => p.HasSa)
            .Select(p => new SaParticipantDto(p.UserId, p.FullName, p.Email, p.Phone))
            .ToList();
        var notCompleted = participants
            .Where(p => !p.HasSa)
            .Select(p => new SaParticipantDto(p.UserId, p.FullName, p.Email, p.Phone))
            .ToList();

        return new CampSaStatusDto
        {
            RequiresSelfAssessment = camp.RequiresSelfAssessment,
            TotalParticipants = participants.Count,
            CompletedCount = completed.Count,
            NotCompletedCount = notCompleted.Count,
            Completed = completed,
            NotCompleted = notCompleted
        };
    }
}
