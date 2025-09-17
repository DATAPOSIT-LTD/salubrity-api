using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthAssesment;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Join;
// adjust to your EF DbContext namespace:
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Camps;

public class MyCampReadRepository : IMyCampReadRepository
{
    private readonly AppDbContext _db; // or whatever your concrete DbContext is named
    private readonly IPackageReferenceResolver _referenceResolver;

    public MyCampReadRepository(AppDbContext db, IPackageReferenceResolver referenceResolver)
    {
        _db = db;
        _referenceResolver = referenceResolver;
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

        // Load all assignments
        var assignments = await _db.Set<HealthCampServiceAssignment>()
            .AsNoTracking()
            .Where(a => a.HealthCampId == campId)
            .Include(a => a.Subcontractor).ThenInclude(s => s.User)
            .Include(a => a.Role)
            .ToListAsync(ct);

        var result = new List<MyCampServiceDto>();

        foreach (var a in assignments)
        {
            var name = await _referenceResolver.GetNameAsync((PackageItemType)a.AssignmentType, a.AssignmentId);
            string? description = await _referenceResolver.GetDescriptionAsync((PackageItemType)a.AssignmentType, a.AssignmentId);

            result.Add(new MyCampServiceDto
            {
                CampAssignmentId = a.Id,

                ServiceId = a.AssignmentId,
                ServiceName = name,
                Description = description,
                StationName = name,

                SubcontractorId = a.SubcontractorId,
                ServedBy = a.Subcontractor?.User?.FullName,

                ProfessionId = a.ProfessionId,
                Profession = a.Role?.Name
            });
        }

        return result;
    }


}
