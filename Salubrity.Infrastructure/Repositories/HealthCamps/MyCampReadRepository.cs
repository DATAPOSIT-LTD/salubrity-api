using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
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
    private readonly IIntakeFormResponseRepository _intakeFormResponsesRepo;
    private readonly IHealthCampParticipantRepository _healthCampParticipantRepository;

    public MyCampReadRepository(AppDbContext db, IPackageReferenceResolver referenceResolver, IIntakeFormResponseRepository intakeFormResponsesRepo, IHealthCampParticipantRepository healthCampParticipantRepository)
    {
        _db = db;
        _referenceResolver = referenceResolver;
        _intakeFormResponsesRepo = intakeFormResponsesRepo;
        _healthCampParticipantRepository = healthCampParticipantRepository;
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
    Guid userId,
    Guid campId,
    bool group = false,
    CancellationToken ct = default)
    {
        // Step 1: Ensure user is a participant
        var isParticipant = await _db.Set<HealthCampParticipant>()
            .AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.HealthCampId == campId, ct);

        if (!isParticipant) return Array.Empty<MyCampServiceDto>();

        var participant = await _db.HealthCampParticipants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        if (participant == null)
            return Array.Empty<MyCampServiceDto>();

        var patientId = await _healthCampParticipantRepository
            .GetPatientIdByParticipantIdAsync(participant.Id, ct);

        if (patientId == null)
            return Array.Empty<MyCampServiceDto>();

        // Step 2: Load assignments
        var assignments = await _db.Set<HealthCampServiceAssignment>()
            .AsNoTracking()
            .Where(a => a.HealthCampId == campId)
            .Include(a => a.Subcontractor).ThenInclude(s => s.User)
            .Include(a => a.Role)
            .ToListAsync(ct);

        var responses = await _intakeFormResponsesRepo
            .GetResponsesByPatientAndCampIdAsync(patientId, campId, ct);

        var result = new List<MyCampServiceDto>();

        foreach (var a in assignments)
        {
            var type = (PackageItemType)a.AssignmentType;
            var name = await _referenceResolver.GetNameAsync(type, a.AssignmentId);
            var description = await _referenceResolver.GetDescriptionAsync(type, a.AssignmentId);

            var isCompleted = responses.Any(r =>
                r.ServiceId == a.AssignmentId && r.Status.Name == "Submitted");

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
                Profession = a.Role?.Name,
                IsCompleted = isCompleted,
                Children = new List<MyCampServiceDto>()
            });
        }

        if (!group)
            return result;

        // Step 3: Safe grouping by ServiceId
        var groupedList = new List<MyCampServiceDto>();

        // Group by ServiceId (handle duplicates)
        var serviceBuckets = result
            .GroupBy(r => r.ServiceId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var groupItem in serviceBuckets)
        {
            var primary = groupItem.Value.First();
            primary.Children = groupItem.Value.Skip(1).ToList();

            var (parentId, _) = await _referenceResolver.GetParentAsync(primary.ServiceId);

            if (parentId != null)
            {
                if (serviceBuckets.TryGetValue(parentId.Value, out var parentGroup))
                {
                    var parentPrimary = parentGroup.First();
                    parentPrimary.Children.Add(primary);
                }
                else
                {
                    // Parent not present in assignment list → still include as top-level
                    groupedList.Add(primary);
                }
            }
            else
            {
                // No parent → root node
                groupedList.Add(primary);
            }
        }

        return groupedList;
    }


    // public async Task<IReadOnlyList<MyCampServiceDto>> GetServicesForUserCampAsync(
    //     Guid userId, Guid campId, CancellationToken ct = default)
    // {
    //     // Ensure user is a participant of this camp (guards data leakage)
    //     var isParticipant = await _db.Set<HealthCampParticipant>()
    //         .AsNoTracking()
    //         .AnyAsync(p => p.UserId == userId && p.HealthCampId == campId, ct);

    //     if (!isParticipant) return Array.Empty<MyCampServiceDto>();

    //     var participant = await _db.HealthCampParticipants.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken: ct);

    //     if (participant == null)
    //         return Array.Empty<MyCampServiceDto>();

    //     var patientId = await _healthCampParticipantRepository.GetPatientIdByParticipantIdAsync(participant.Id, ct);
    //     if (patientId == null)
    //         return Array.Empty<MyCampServiceDto>();
    //     // Load all assignments
    //     var assignments = await _db.Set<HealthCampServiceAssignment>()
    //     .AsNoTracking()
    //     .Where(a => a.HealthCampId == campId)
    //     .Include(a => a.Subcontractor).ThenInclude(s => s.User)
    //     .Include(a => a.Role)
    //     .ToListAsync(ct);

    //     var result = new List<MyCampServiceDto>();

    //     foreach (var a in assignments)
    //     {
    //         var name = await _referenceResolver.GetNameAsync((PackageItemType)a.AssignmentType, a.AssignmentId);
    //         string? description = await _referenceResolver.GetDescriptionAsync((PackageItemType)a.AssignmentType, a.AssignmentId);

    //         var res = await _intakeFormResponsesRepo.GetResponsesByPatientAndCampIdAsync(patientId, campId, ct);
    //         var isCompleted = false;
    //         foreach (var r in res)
    //         {
    //             if (r.ServiceId == a.AssignmentId && r.Status.Name == "Submitted")
    //             {
    //                 isCompleted = true;
    //             }
    //         }
    //         result.Add(new MyCampServiceDto
    //         {
    //             CampAssignmentId = a.Id,

    //             ServiceId = a.AssignmentId,
    //             ServiceName = name,
    //             Description = description,
    //             StationName = name,

    //             SubcontractorId = a.SubcontractorId,
    //             ServedBy = a.Subcontractor?.User?.FullName,

    //             ProfessionId = a.ProfessionId,
    //             Profession = a.Role?.Name,
    //             IsCompleted = isCompleted
    //         });
    //     }

    //     return result;
    // }


}
