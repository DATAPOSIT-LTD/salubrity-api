using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Domain.Entities.HealthAssesment;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Join;
// adjust to your EF DbContext namespace:
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Camps;

public class MyCampReadRepository : IMyCampReadRepository
{
    private readonly AppDbContext _db; // or whatever your concrete DbContext is named

    public MyCampReadRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PagedResult<MyCampListItemDto>> GetUpcomingForUserAsync(
        Guid userId, int page, int pageSize, string? search, CancellationToken ct = default)
    {
        page = page <= 0 ? 1 : page;
        pageSize = pageSize <= 0 ? 10 : Math.Min(pageSize, 100);

        var today = DateTime.UtcNow.Date;

        var baseQuery = _db.Set<HealthCampParticipant>()
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.HealthCamp)
            .Where(c => c != null && c.IsLaunched)
            .Where(c =>
                   c!.StartDate.Date >= today
                || (c.StartDate.Date <= today && (c.EndDate == null || c.EndDate.Value.Date >= today)))
            .Distinct();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            baseQuery = baseQuery.Where(c =>
                (c!.Name != null && EF.Functions.ILike(c.Name, s)) ||
                (c!.Organization != null && c.Organization.BusinessName != null && EF.Functions.ILike(c.Organization.BusinessName, s)) ||
                (c!.Location != null && EF.Functions.ILike(c.Location, s)));
        }

        var total = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderBy(c => c!.StartDate)
            .Select(c => new MyCampListItemDto
            {
                CampId = c!.Id,
                CampName = c.Name,
                Organization = c.Organization != null ? c.Organization.BusinessName : null,
                PackageServices = c.ServicePackage != null ? c.ServicePackage.Name : null,
                NumberOfServices = c.ServiceAssignments != null ? c.ServiceAssignments.Count : 0,
                Venue = c.Location ?? (c.Location != null ? c.Location : null),
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Status = c.HealthCampStatus != null && c.HealthCampStatus.Name != null
                    ? c.HealthCampStatus.Name
                    : (c.StartDate.Date <= today && (c.EndDate == null || c.EndDate.Value.Date >= today))
                        ? "Ongoing"
                        : "Planned"
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<MyCampListItemDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
    }

    public async Task<IReadOnlyList<MyCampServiceDto>> GetServicesForUserCampAsync(
         Guid userId, Guid campId, CancellationToken ct = default)
    {
        // Ensure user is a participant of this camp (guards data leakage)
        var isParticipant = await _db.Set<HealthCampParticipant>()
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.HealthCampId == campId, ct);

        if (!isParticipant) return Array.Empty<MyCampServiceDto>();

        // Stations = assignments for this camp
        var items = await _db.Set<HealthCampServiceAssignment>()
            .AsNoTracking()
            .Where(a => a.HealthCampId == campId)
            .Select(a => new MyCampServiceDto
            {
                ServiceId = a.ServiceId,
                ServiceName = a.Service.Name,
                Description = a.Service.Description,
                CampAssignmentId = a.Id,


                SubcontractorId = a.SubcontractorId,
                ServedBy = a.Subcontractor.User.FullName,       // change if your field differs

                ProfessionId = a.ProfessionId,
                Profession = a.Role != null ? a.Role.Name : null,

                StationName = a.Service.Name                   // no alias column yet
            })
            .ToListAsync(ct);

        return items;
    }

}
