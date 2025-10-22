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

        //  Get base query for user's camps
        var baseQuery = _db.Set<HealthCampParticipant>()
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.HealthCamp)
            .Where(c => c != null && c.IsLaunched && !c.IsDeleted)
            .Where(c =>
                   c!.StartDate.Date >= today
                || (c.StartDate.Date <= today && (c.EndDate == null || c.EndDate.Value.Date >= today)))
            .Distinct();

        //  Optional search
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = $"%{search.Trim()}%";
            baseQuery = baseQuery.Where(c =>
                (c!.Name != null && EF.Functions.ILike(c.Name, s)) ||
                (c!.Organization != null && c.Organization.BusinessName != null && EF.Functions.ILike(c.Organization.BusinessName, s)) ||
                (c!.Location != null && EF.Functions.ILike(c.Location, s)));
        }

        var total = await baseQuery.CountAsync(ct);

        // Now project efficiently using HealthCampPackages
        var items = await baseQuery
            .OrderBy(c => c!.StartDate)
            .Select(c => new MyCampListItemDto
            {
                CampId = c!.Id,
                CampName = c.Name,
                Organization = c.Organization != null ? c.Organization.BusinessName : null,

                // Corrected for new model: pick first active package name
                PackageServices = c.HealthCampPackages
                    .Where(p => p.IsActive)
                    .Select(p => p.ServicePackage.Name)
                    .FirstOrDefault(),

                NumberOfServices = c.ServiceAssignments != null ? c.ServiceAssignments.Count : 0,
                Venue = c.Location,
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
        var participant = await _db.HealthCampParticipants
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.HealthCampId == campId, ct);

        if (participant == null)
            return [];

        var patientId = await _healthCampParticipantRepository
            .GetPatientIdByParticipantIdAsync(participant.Id, ct);

        if (patientId == null)
            return [];

        // Step 2: Get participant’s active package assignment
        var participantPackage = await _db.Set<HealthCampParticipantPackage>()
            .AsNoTracking()
            .Include(pp => pp.HealthCampPackage)
                .ThenInclude(cp => cp.ServicePackage)
            .FirstOrDefaultAsync(pp =>
                pp.ParticipantId == participant.Id &&
                pp.IsActive, ct);

        // 🧩 if participant has no active package → return empty
        if (participantPackage?.HealthCampPackageId == null)
            return [];

        var participantPackageId = participantPackage.HealthCampPackageId;
        var participantServicePackageId = participantPackage.HealthCampPackage.ServicePackageId;

        // Step 3: Prefetch package items and packages to avoid N+1 queries
        var campPackageItems = await _db.Set<HealthCampPackageItem>()
            .AsNoTracking()
            .Where(i =>
                i.HealthCampId == campId &&
                i.ServicePackageId == participantServicePackageId) // only items under participant’s package
            .ToListAsync(ct);

        var campPackages = await _db.Set<HealthCampPackage>()
            .AsNoTracking()
            .Include(p => p.ServicePackage)
            .Where(p =>
                p.HealthCampId == campId &&
                p.Id == participantPackageId) // only participant’s package
            .ToListAsync(ct);

        // Step 4: Load service assignments for this camp
        // but only for services included in the participant's package items
        var serviceIds = campPackageItems.Select(i => i.ReferenceId).ToList();


        var assignments = await _db.Set<HealthCampServiceAssignment>()
            .AsNoTracking()
            .Where(a =>
                a.HealthCampId == campId &&
                serviceIds.Contains(a.AssignmentId))
            .Include(a => a.Subcontractor).ThenInclude(s => s.User)
            .Include(a => a.Role)
            .GroupBy(a => a.AssignmentId)
            .Select(g => g.First())
            .ToListAsync(ct);



        // Step 5: Load participant responses (for completion check)
        var responses = await _intakeFormResponsesRepo
            .GetResponsesByPatientAndCampIdAsync(patientId, campId, ct);

        // Step 6: Build result list
        var result = new List<MyCampServiceDto>();

        foreach (var a in assignments)
        {
            var type = (PackageItemType)a.AssignmentType;
            var name = await _referenceResolver.GetNameAsync(type, a.AssignmentId);
            var description = await _referenceResolver.GetDescriptionAsync(type, a.AssignmentId);

            var isCompleted = responses.Any(r =>
                r.ServiceId == a.AssignmentId && r.Status.Name == "Submitted");

            // Match the package this item belongs to (only one at this point)
            var campItem = campPackageItems.FirstOrDefault(i =>
                i.ReferenceId == a.AssignmentId && i.ReferenceType == type);

            var resolvedPackage = campPackages.FirstOrDefault();

            // 🧾 Build DTO
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

                // Package info: limited to participant’s assigned package
                HealthCampPackageId = resolvedPackage?.Id ?? participantPackageId,
                PackageName =
                    resolvedPackage?.DisplayName ??
                    resolvedPackage?.ServicePackage?.Name ??
                    participantPackage?.HealthCampPackage?.DisplayName ??
                    participantPackage?.HealthCampPackage?.ServicePackage?.Name ??
                    "Unassigned",

                Children = new List<MyCampServiceDto>()
            });
        }

        // Step 7: Return directly if grouping not requested
        if (!group)
            return result;

        // Step 8: Safe hierarchical grouping
        var groupedList = new List<MyCampServiceDto>();
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
                    groupedList.Add(primary);
                }
            }
            else
            {
                groupedList.Add(primary);
            }
        }

        return groupedList;
    }





}
