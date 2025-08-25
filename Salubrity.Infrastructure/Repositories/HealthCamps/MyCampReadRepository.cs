using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Camps;
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
                PackageServices = c.ServicePackage.Name,
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
}
