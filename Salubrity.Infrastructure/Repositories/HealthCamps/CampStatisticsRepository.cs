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
                Vendors = 0,
                Visitors = 0
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
}
