using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Forms;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Enums;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Domain.Entities.Join;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Shared.Constants;
using Salubrity.Shared.Exceptions;
using Scriban.Syntax;

namespace Salubrity.Infrastructure.Repositories.HealthCamps;

public class HealthCampRepository : IHealthCampRepository
{
    private readonly AppDbContext _context;
    private readonly IPackageReferenceResolver _referenceResolver;
    private readonly IMapper _mapper;
    private readonly IServiceRepository _serviceRepo;

    private readonly IServiceCategoryRepository _categoryRepo;
    private readonly IServiceSubcategoryRepository _subcategoryRepo;



    public HealthCampRepository(AppDbContext context, IPackageReferenceResolver referenceResolver, IMapper mapper, IServiceRepository serviceRepository, IServiceCategoryRepository categoryRepository, IServiceSubcategoryRepository serviceSubcategory)
    {
        _context = context;
        _referenceResolver = referenceResolver;
        _mapper = mapper;
        _categoryRepo = categoryRepository;
        _serviceRepo = serviceRepository;
        _subcategoryRepo = serviceSubcategory;
    }

    public async Task<HealthCamp?> GetBySlugAsync(
    string slug,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return await _context.HealthCamps
            .AsNoTracking()
            .FirstOrDefaultAsync(
                c => c.Slug.ToLower() == normalizedSlug && !c.IsDeleted,
                ct
            );
    }


    public async Task<List<HealthCampListDto>> GetAllAsync()
    {
        var camps = await _context.HealthCamps
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .Include(c => c.Organization)
            .Include(c => c.HealthCampPackages)
                .ThenInclude(p => p.ServicePackage)
            .Include(c => c.ServiceAssignments)
            .Include(c => c.HealthCampStatus)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var list = new List<HealthCampListDto>(camps.Count);

        foreach (var c in camps)
        {
            // Choose the *active* package to display in list view
            var activePackage = c.HealthCampPackages.FirstOrDefault(p => p.IsActive);

            list.Add(new HealthCampListDto
            {
                Id = c.Id,
                ClientName = c.Organization?.BusinessName ?? "N/A",
                ExpectedPatients = c.ExpectedParticipants ?? 0,
                Venue = c.Location ?? "N/A",
                DateRange = $"{c.StartDate:dd} - {c.EndDate:dd MMM, yyyy}",
                SubcontractorCount = c.ServiceAssignments?.Count ?? 0,
                Status = c.HealthCampStatus?.Name ?? "Unknown",
                PackageName = activePackage?.ServicePackage?.Name ?? "N/A"
            });
        }

        return list;
    }


    public async Task<HealthCampDetailDto?> GetCampDetailsByIdAsync(Guid id)
    {
        var camp = await _context.HealthCamps
            .AsNoTracking()
            .Include(c => c.Organization)
                .ThenInclude(o => o.InsuranceProviders)
                    .ThenInclude(ip => ip.InsuranceProvider)
            .Include(c => c.HealthCampPackages)
                .ThenInclude(p => p.ServicePackage)
            .Include(c => c.PackageItems)
            .Include(c => c.ServiceAssignments)
            .Include(c => c.HealthCampStatus)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (camp is null)
            return null;

        // Pick active package (the “chosen one”)
        var activePackage = camp.HealthCampPackages.FirstOrDefault(p => p.IsActive);

        var dto = new HealthCampDetailDto
        {
            Id = camp.Id,
            Name = camp.Name,
            StartDate = camp.StartDate,
            ClientName = camp.Organization?.BusinessName ?? "N/A",
            Venue = camp.Location ?? "N/A",
            ExpectedPatients = camp.ExpectedParticipants ?? 0,
            SubcontractorCount = camp.ServiceAssignments?.Count ?? 0,

            // Reflect new structure
            PackageName = activePackage?.ServicePackage?.Name ?? "N/A",
            PackageCost = activePackage?.ServicePackage?.Price,
            Status = camp.HealthCampStatus?.Name ?? "Unknown",
            InsurerName = camp.Organization?.InsuranceProviders?
                .FirstOrDefault()?.InsuranceProvider?.Name ?? string.Empty,

            ServiceStations = new List<ServiceStationDto>()
        };

        // Resolve package item names concurrently
        var itemTasks = camp.PackageItems
            .Where(pi => !pi.IsDeleted)
            .Select(async pi =>
            {
                var name = await _referenceResolver.GetNameAsync(pi.ReferenceType, pi.ReferenceId);
                return new ServiceStationDto
                {
                    Id = pi.Id,
                    Name = name,
                    PatientsServed = 0,     // TODO: replace with live metrics
                    PendingService = 0,     // TODO: replace with live metrics
                    AvgTimePerPatient = "0 min",
                    OutlierAlerts = 0
                };
            });

        dto.ServiceStations.AddRange(await Task.WhenAll(itemTasks));

        return dto;
    }

    public async Task<HealthCamp?> GetByIdAsync(Guid id)
    {
        return await _context.HealthCamps
            .Include(c => c.PackageItems)
            .Include(c => c.ServiceAssignments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<HealthCamp> CreateAsync(HealthCamp entity)
    {
        _context.HealthCamps.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<HealthCamp> UpdateAsync(HealthCamp entity)
    {
        _context.HealthCamps.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var camp = await _context.HealthCamps.FindAsync(id);
        if (camp != null)
        {
            camp.IsDeleted = true;
            camp.DeletedAt = DateTime.UtcNow;
            _context.HealthCamps.Update(camp);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<HealthCamp?> GetForLaunchAsync(Guid id)
    {
        return await _context.HealthCamps
            .Include(c => c.Participants)
                .ThenInclude(p => p.User)
            .Include(c => c.HealthCampStatus)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }



    public async Task UpsertTempCredentialAsync(HealthCampTempCredentialUpsert upsert)
    {
        var existing = await _context.HealthCampTempCredentials
            .FirstOrDefaultAsync(x =>
                x.HealthCampId == upsert.HealthCampId &&
                x.UserId == upsert.UserId &&
                x.Role == upsert.Role &&
                !x.IsDeleted);

        if (existing is null)
        {
            var entity = new HealthCampTempCredential
            {
                Id = Guid.NewGuid(),
                HealthCampId = upsert.HealthCampId,
                UserId = upsert.UserId,
                Role = upsert.Role,
                TempPasswordHash = upsert.TempPasswordHash,
                TempPasswordExpiresAt = upsert.TempPasswordExpiresAt,
                SignInJti = upsert.SignInJti,
                TokenExpiresAt = upsert.TokenExpiresAt
            };

            _context.HealthCampTempCredentials.Add(entity);
        }
        else
        {
            existing.TempPasswordHash = upsert.TempPasswordHash;
            existing.TempPasswordExpiresAt = upsert.TempPasswordExpiresAt;
            existing.SignInJti = upsert.SignInJti;
            existing.TokenExpiresAt = upsert.TokenExpiresAt;
            _context.HealthCampTempCredentials.Update(existing);
        }

        await _context.SaveChangesAsync();
    }


    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }



    public async Task<List<HealthCampListDto>> GetMyCanceledCampsAsync(Guid subcontractorId)
    {
        var items = await _context.HealthCampServiceAssignments
            .Where(x => x.SubcontractorId == subcontractorId)
            .Select(x => x.HealthCamp)
            .Where(c => !c.IsLaunched || c.HealthCampStatus!.Name == "Suspended")
            .Include(c => c.Organization)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<HealthCampListDto>>(items);
    }


    private IQueryable<HealthCamp> CampsForSubcontractor(Guid? subcontractorId)
    {
        if (subcontractorId == null)
            throw new ArgumentNullException(nameof(subcontractorId));

        return _context.SubcontractorHealthCampAssignments
            .Where(a =>
                a.SubcontractorId == subcontractorId &&
                !a.IsDeleted &&
                !a.HealthCamp.IsDeleted
            )
            .Select(a => a.HealthCamp)
            .Distinct()
            .Include(c => c.Organization)
            .AsNoTracking()
            .AsSplitQuery();
    }

    public async Task<List<HealthCamp>> GetMyUpcomingCampsAsync(Guid? subcontractorId, CancellationToken ct = default)
    {
        var eat = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
        var nowUtc = DateTime.UtcNow;
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, eat).Date;
        if (subcontractorId == null)
        {
            return await _context.HealthCamps
            .Where(c =>
                (c.CloseDate == null || c.CloseDate > nowUtc) && !c.IsDeleted &&
                (
                    c.StartDate >= todayLocal ||
                    (c.IsLaunched &&
                     c.StartDate <= todayLocal &&
                     (c.EndDate ?? c.StartDate) >= todayLocal)
                )
            )
            .Include(c => c.HealthCampStatus)
            .Include(c => c.Organization)
            .Include(c => c.ServiceAssignments)
            .AsNoTracking()
            .OrderBy(c => c.StartDate)
            .ToListAsync(ct);
        }
        return await CampsForSubcontractor(subcontractorId)
            .Where(c =>
                (c.CloseDate == null || c.CloseDate > nowUtc) && !c.IsDeleted &&
                (
                    c.StartDate >= todayLocal ||
                    (c.IsLaunched &&
                     c.StartDate <= todayLocal &&
                     (c.EndDate ?? c.StartDate) >= todayLocal)
                )
            )
            .Include(c => c.HealthCampStatus)
            .Include(c => c.Organization)
            .Include(c => c.ServiceAssignments)
            .AsNoTracking()
            .OrderBy(c => c.StartDate)
            .ToListAsync(ct);
    }

    public async Task<List<HealthCamp>> GetMyCompleteCampsAsync(Guid subcontractorId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        return await CampsForSubcontractor(subcontractorId)
            .Where(c => c.IsLaunched && !c.IsDeleted
                        && ((c.EndDate ?? c.StartDate) < today))
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);
    }

    public async Task<List<HealthCamp>> GetMyCanceledCampsAsync(Guid subcontractorId, CancellationToken ct = default)
    {
        return await CampsForSubcontractor(subcontractorId)
            .Where(c => !c.IsDeleted)
            .Where(c =>
                !c.IsLaunched ||
                (c.HealthCampStatus != null && c.HealthCampStatus.Name == "Suspended")
            // or: new[] { "Suspended", "Incomplete" }.Contains(c.HealthCampStatus!.Name)
            )
            .Distinct()
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);
    }


    // 2) Keep BaseParticipants pure IQueryable over the entity (no projection)
    private IQueryable<HealthCampParticipant> BaseParticipants(Guid campId, string? q, string? sort)
    {
        var query = _context.HealthCampParticipants
            .Where(p => p.HealthCampId == campId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p =>
                (p.User.FullName != null && EF.Functions.ILike(p.User.FullName, $"%{term}%")) ||
                (p.User.Email != null && EF.Functions.ILike(p.User.Email, $"%{term}%")) ||
                (p.User.Phone != null && EF.Functions.ILike(p.User.Phone, $"%{term}%")));
        }

        query = sort?.ToLowerInvariant() switch
        {
            "name" => query.OrderBy(p => p.User.FullName),
            "oldest" => query.OrderBy(p => p.CreatedAt),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        // Include what you display; EF can also translate without Include, but this avoids lazy loads later
        return query
            .Include(p => p.User)
            .Include(p => p.HealthCamp)
                .ThenInclude(h => h.Organization)
            .AsNoTracking();
    }


    // 3) Materialize and inject PatientId in one DB round-trip for Patients
    public async Task<List<HealthCampParticipant>> GetParticipantsAsync(Guid campId, string? q, string? sort, CancellationToken ct = default)
    {
        return await BaseParticipants(campId, q, sort).ToListAsync(ct);
    }

    public async Task<HealthCamp?> GetByIdWithPackagesAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.HealthCamps
            .Include(c => c.HealthCampPackages)
                .ThenInclude(p => p.ServicePackage)
            .Include(c => c.ServiceAssignments)
            .Include(c => c.Participants)
            .AsSplitQuery()
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted, ct);
    }



    private static IQueryable<CampParticipantListDto> Project(
        IQueryable<HealthCampParticipant> q,
        int totalAssignments)
    {
        return q.Select(p => new CampParticipantListDto
        {
            Id = p.Id,
            UserId = p.UserId,
            PatientId = p.PatientId,
            FullName = p.User.FullName!,
            Email = p.User.Email,
            PhoneNumber = p.User.Phone,
            CompanyName = p.HealthCamp.Organization.BusinessName,
            ParticipatedAt = p.ParticipatedAt,
            Served = p.ServiceStatuses.Count(s => s.ServedAt != null) >= totalAssignments
        });
    }


    public IQueryable<CampParticipantListDto> BaseParticipantsDto(
    Guid campId,
    string? q,
    string? sort,
    Guid? serviceAssignmentId = null)
    {
        int totalAssignments = 0;
        if (serviceAssignmentId == null)
        {
            totalAssignments = _context.HealthCampServiceAssignments
                .Count(a => a.HealthCampId == campId && !a.IsDeleted);
        }

        var query =
            from p in _context.HealthCampParticipants
            where p.HealthCampId == campId
            select new CampParticipantListDto
            {
                Id = p.Id,
                UserId = p.UserId,
                PatientId = _context.Patients
                    .Where(pa => pa.UserId == p.UserId && !pa.IsDeleted)
                    .Select(pa => pa.Id)
                    .FirstOrDefault(),
                FullName = p.User.FullName!,
                Email = p.User.Email,
                PhoneNumber = p.User.Phone,
                CompanyName = p.HealthCamp.Organization.BusinessName!,
                ParticipatedAt = p.ParticipatedAt,
                PackageId = p.HealthCampPackage != null ? p.HealthCampPackage.ServicePackage.Id : (Guid?)null,   // <-- Added
                PackageName = p.HealthCampPackage != null ? p.HealthCampPackage.ServicePackage.Name : null,      // <-- Added
                Served = serviceAssignmentId != null
                    ? _context.HealthCampParticipantServiceStatuses
                        .Any(s =>
                            s.ParticipantId == p.Id &&
                            s.ServiceAssignmentId == serviceAssignmentId &&
                            s.ServedAt != null)
                    : _context.HealthCampParticipantServiceStatuses
                        .Count(s => s.ParticipantId == p.Id && s.ServedAt != null) >= totalAssignments
            };

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                (x.FullName != null && EF.Functions.ILike(x.FullName, $"%{term}%")) ||
                (x.Email != null && EF.Functions.ILike(x.Email, $"%{term}%")) ||
                (x.PhoneNumber != null && EF.Functions.ILike(x.PhoneNumber, $"%{term}%")));
        }

        var s = sort?.ToLowerInvariant();
        query = s switch
        {
            "name" => query.OrderBy(x => x.FullName),
            "oldest" => query.OrderBy(x => x.ParticipatedAt == null)
                             .ThenBy(x => x.ParticipatedAt),
            _ => query.OrderByDescending(x => x.ParticipatedAt != null)
                             .ThenByDescending(x => x.ParticipatedAt)
        };

        return query.AsNoTracking();
    }

    // =========================================================
    // INTERNAL: service-scoped participant query engine
    // =========================================================
    // private async Task<PagedResult<CampParticipantListDto>> GetCampParticipantsByResolvedServiceAsync(
    //     Guid campId,
    //     Guid serviceReferenceId, // service / category / subcategory
    //     Guid participantId, // Filter by participant
    //     CampParticipantServeStatus status,
    //     string? q,
    //     string? sort,
    //     int page,
    //     int pageSize,
    //     CancellationToken ct)
    // {
    //     if (page <= 0) page = 1;
    //     if (pageSize <= 0) pageSize = 20;

    //     // --------------------------------------------------
    //     // Resolve camp-scoped assignment
    //     // --------------------------------------------------
    //     var assignment = await _context.HealthCampServiceAssignments
    //         .AsNoTracking()
    //         .Where(a =>
    //             a.HealthCampId == campId &&
    //             !a.IsDeleted &&
    //             a.AssignmentId == serviceReferenceId)
    //         .Select(a => new { a.AssignmentId, a.AssignmentType })
    //         .FirstOrDefaultAsync(ct);

    //     if (assignment == null)
    //         throw new InvalidOperationException(
    //             $"No HealthCampServiceAssignment found for service reference {serviceReferenceId} in camp {campId}");

    //     // --------------------------------------------------
    //     // Resolve ROOT ServiceId (authoritative)
    //     // --------------------------------------------------
    //     Guid resolvedServiceId = assignment.AssignmentType switch
    //     {
    //         PackageItemType.Service =>
    //             assignment.AssignmentId,

    //         PackageItemType.ServiceCategory =>
    //             await _context.ServiceCategories
    //                 .Where(c => c.Id == assignment.AssignmentId)
    //                 .Select(c => c.ServiceId)
    //                 .FirstAsync(ct),

    //         PackageItemType.ServiceSubcategory =>
    //             await _context.ServiceSubcategories
    //                 .Where(sc => sc.Id == assignment.AssignmentId)
    //                 .Select(sc => sc.ServiceCategory.ServiceId)
    //                 .FirstAsync(ct),

    //         _ => throw new InvalidOperationException(
    //             $"Unsupported assignment type: {assignment.AssignmentType}")
    //     };

    //     // --------------------------------------------------
    //     // Participant projection (service-scoped)
    //     // --------------------------------------------------
    //     var query =
    //         from p in _context.HealthCampParticipants
    //         where p.HealthCampId == campId

    //         let patientId =
    //             _context.Patients
    //                 .Where(pa => pa.UserId == p.UserId && !pa.IsDeleted)
    //                 .Select(pa => pa.Id)
    //                 .FirstOrDefault()

    //         let served =
    //             _context.IntakeFormResponses.Any(r =>
    //                 r.PatientId == patientId &&
    //                 r.ResolvedServiceId == resolvedServiceId &&
    //                  r.HealthCampId == campId
    //                 )

    //         select new CampParticipantListDto
    //         {
    //             Id = p.Id,
    //             UserId = p.UserId,
    //             PatientId = patientId,
    //             FullName = p.User.FullName!,
    //             Email = p.User.Email,
    //             PhoneNumber = p.User.Phone,
    //             CompanyName = p.HealthCamp.Organization.BusinessName!,
    //             ParticipatedAt = p.ParticipatedAt,
    //             Served = served,
    //             CompletedServices = new() // not used in this mode
    //         };

    //     if (participantId != Guid.Empty)
    //     {
    //         query = query.Where(x => x.Id == participantId);
    //     }

    //     // --------------------------------------------------
    //     // Status filter
    //     // --------------------------------------------------
    //     query = status switch
    //     {
    //         CampParticipantServeStatus.Served => query.Where(x => x.Served == true),
    //         CampParticipantServeStatus.NotServed => query.Where(x => x.Served == false),
    //         _ => query
    //     };

    //     // --------------------------------------------------
    //     // Search
    //     // --------------------------------------------------
    //     if (!string.IsNullOrWhiteSpace(q))
    //     {
    //         var term = q.Trim();
    //         query = query.Where(x =>
    //             (x.FullName != null && EF.Functions.ILike(x.FullName, $"%{term}%")) ||
    //             (x.Email != null && EF.Functions.ILike(x.Email, $"%{term}%")) ||
    //             (x.PhoneNumber != null && EF.Functions.ILike(x.PhoneNumber, $"%{term}%")));
    //     }

    //     // --------------------------------------------------
    //     // Sort
    //     // --------------------------------------------------
    //     query = sort?.ToLowerInvariant() switch
    //     {
    //         "name" => query.OrderBy(x => x.FullName),
    //         "oldest" => query.OrderBy(x => x.ParticipatedAt),
    //         _ => query.OrderByDescending(x => x.ParticipatedAt)
    //     };

    //     // --------------------------------------------------
    //     // Pagination
    //     // --------------------------------------------------
    //     var total = await query.CountAsync(ct);

    //     var items = await query
    //         .Skip((page - 1) * pageSize)
    //         .Take(pageSize)
    //         .AsNoTracking()
    //         .ToListAsync(ct);

    //     return new PagedResult<CampParticipantListDto>
    //     {
    //         Page = page,
    //         PageSize = pageSize,
    //         Total = total,
    //         Items = items
    //     };
    // }

    private async Task<PagedResult<CampParticipantListDto>> GetCampParticipantsByResolvedServiceAsync(
    Guid campId,
    Guid serviceReferenceId, // service / category / subcategory
    Guid participantId,      // optional filter (Guid.Empty = all)
    CampParticipantServeStatus status,
    string? q,
    string? sort,
    int page,
    int pageSize,
    CancellationToken ct)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        // --------------------------------------------------
        // 1. Resolve camp-scoped assignment (by AssignmentId or by resolved Service.Id)
        // --------------------------------------------------
        var assignment = await _context.HealthCampServiceAssignments
            .AsNoTracking()
            .Where(a =>
                a.HealthCampId == campId &&
                !a.IsDeleted &&
                a.AssignmentId == serviceReferenceId)
            .Select(a => new { a.AssignmentId, a.AssignmentType })
            .FirstOrDefaultAsync(ct);

        if (assignment == null)
        {
            // Frontend may send resolved Service.Id; find assignment that resolves to this service
            var assignmentByResolvedService = await _context.HealthCampServiceAssignments
                .AsNoTracking()
                .Where(a => a.HealthCampId == campId && !a.IsDeleted)
                .Select(a => new { a.AssignmentId, a.AssignmentType })
                .ToListAsync(ct);

            foreach (var a in assignmentByResolvedService)
            {
                Guid? resolvedSvcId = a.AssignmentType switch
                {
                    PackageItemType.Service => a.AssignmentId,
                    PackageItemType.ServiceCategory => await _context.ServiceCategories
                        .Where(c => c.Id == a.AssignmentId)
                        .Select(c => (Guid?)c.ServiceId)
                        .FirstOrDefaultAsync(ct),
                    PackageItemType.ServiceSubcategory => await _context.ServiceSubcategories
                        .Where(sc => sc.Id == a.AssignmentId)
                        .Select(sc => (Guid?)sc.ServiceCategory.ServiceId)
                        .FirstOrDefaultAsync(ct),
                    _ => null
                };
                if (resolvedSvcId == serviceReferenceId)
                {
                    assignment = a;
                    break;
                }
            }
        }

        if (assignment == null)
            throw new InvalidOperationException(
                $"No HealthCampServiceAssignment found for service reference {serviceReferenceId} in camp {campId}");

        // --------------------------------------------------
        // 2. Resolve ROOT ServiceId (authoritative)
        // --------------------------------------------------
        Guid resolvedServiceId = assignment.AssignmentType switch
        {
            PackageItemType.Service =>
                assignment.AssignmentId,

            PackageItemType.ServiceCategory =>
                await _context.ServiceCategories
                    .Where(c => c.Id == assignment.AssignmentId)
                    .Select(c => c.ServiceId)
                    .FirstAsync(ct),

            PackageItemType.ServiceSubcategory =>
                await _context.ServiceSubcategories
                    .Where(sc => sc.Id == assignment.AssignmentId)
                    .Select(sc => sc.ServiceCategory.ServiceId)
                    .FirstAsync(ct),

            _ => throw new InvalidOperationException(
                $"Unsupported assignment type: {assignment.AssignmentType}")
        };

        // --------------------------------------------------
        // 3. Participant projection (SERVICE-SCOPED + ALLOCATION-SAFE)
        // --------------------------------------------------
        var query =
            from p in _context.HealthCampParticipants.AsNoTracking()
            where p.HealthCampId == campId

            let patientId =
                _context.Patients
                    .Where(pa => pa.UserId == p.UserId && !pa.IsDeleted)
                    .Select(pa => pa.Id)
                    .FirstOrDefault()

            // 3.1 Is this service allocated to this participant?
            let isAllocated =
                p.HealthCampPackageId != null &&
                _context.HealthCampPackageItems.Any(pi =>
                    pi.HealthCampId == campId &&
                    pi.ServicePackageId == p.HealthCampPackage!.ServicePackageId &&
                    (
                        // Direct service
                        (pi.ReferenceType == PackageItemType.Service &&
                         pi.ReferenceId == resolvedServiceId)

                        ||

                        // Category → service
                        (pi.ReferenceType == PackageItemType.ServiceCategory &&
                         _context.ServiceCategories
                            .Where(c => c.Id == pi.ReferenceId)
                            .Select(c => c.ServiceId)
                            .FirstOrDefault() == resolvedServiceId)

                        ||

                        // Subcategory → category → service
                        (pi.ReferenceType == PackageItemType.ServiceSubcategory &&
                         _context.ServiceSubcategories
                            .Where(sc => sc.Id == pi.ReferenceId)
                            .Select(sc => sc.ServiceCategory.ServiceId)
                            .FirstOrDefault() == resolvedServiceId)
                    )
                )

            // 3.2 Has the service actually been served? (IntakeFormResponse = source of truth; this camp only to avoid cross-camp leakage)
            let served =
                _context.IntakeFormResponses.Any(r =>
                    r.PatientId == patientId &&
                    r.ResolvedServiceId == resolvedServiceId &&
                    r.HealthCampId == campId)

            select new CampParticipantListDto
            {
                Id = p.Id,
                UserId = p.UserId,
                PatientId = patientId,
                FullName = p.User.FullName!,
                Email = p.User.Email,
                PhoneNumber = p.User.Phone,
                CompanyName = p.HealthCamp.Organization.BusinessName!,
                ParticipatedAt = p.ParticipatedAt,
                Served = served,
                CompletedServices = new() // not used in service-scoped mode
            };

        // --------------------------------------------------
        // 4. Optional participant filter
        // --------------------------------------------------
        if (participantId != Guid.Empty)
        {
            query = query.Where(x => x.Id == participantId);
        }

        // --------------------------------------------------
        // 5. Status filter
        // --------------------------------------------------
        query = status switch
        {
            CampParticipantServeStatus.Served =>
                query.Where(x => x.Served == true),

            CampParticipantServeStatus.NotServed =>
                query.Where(x => x.Served == false),

            _ => query
        };

        // --------------------------------------------------
        // 6. Search
        // --------------------------------------------------
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                (x.FullName != null && EF.Functions.ILike(x.FullName, $"%{term}%")) ||
                (x.Email != null && EF.Functions.ILike(x.Email, $"%{term}%")) ||
                (x.PhoneNumber != null && EF.Functions.ILike(x.PhoneNumber, $"%{term}%")));
        }

        // --------------------------------------------------
        // 7. Sort
        // --------------------------------------------------
        query = sort?.ToLowerInvariant() switch
        {
            "name" => query.OrderBy(x => x.FullName),
            "oldest" => query.OrderBy(x => x.ParticipatedAt),
            _ => query.OrderByDescending(x => x.ParticipatedAt)
        };

        // --------------------------------------------------
        // 8. Pagination
        // --------------------------------------------------
        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<CampParticipantListDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
    }


    public Task<PagedResult<CampParticipantListDto>> GetCampParticipantsByServiceAsync(
        Guid campId,
        Guid serviceId,
        Guid? participantId, // Filter by participant
        CampParticipantServeStatus status,
        string? q,
        string? sort,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        return GetCampParticipantsByResolvedServiceAsync(
            campId,
            serviceId,
            participantId ?? Guid.Empty,
            status,
            q,
            sort,
            page,
            pageSize,
            ct);
    }


    public async Task<PagedResult<CampParticipantListDto>> GetCampParticipantsCampWideAsync(
      Guid campId,
      Guid? participantId,
      CampParticipantServeStatus status,
      string? q,
      string? sort,
      int page,
      int pageSize,
      CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        // ==================================================
        // 1. Load ALL camp service assignments (camp-wide)
        // ==================================================
        var rawAssignments = await _context.HealthCampServiceAssignments
            .AsNoTracking()
            .Where(a => a.HealthCampId == campId && !a.IsDeleted)
            .Select(a => new { a.Id, a.AssignmentId, a.AssignmentType })
            .ToListAsync(ct);

        var campServices = new List<(Guid AssignmentId, Guid ServiceId, string ServiceName)>();

        foreach (var a in rawAssignments)
        {
            switch (a.AssignmentType)
            {
                case PackageItemType.Service:
                    {
                        var svc = await _context.Services
                            .Where(s => s.Id == a.AssignmentId)
                            .Select(s => new { s.Id, s.Name })
                            .FirstAsync(ct);

                        campServices.Add((a.Id, svc.Id, svc.Name));
                        break;
                    }

                case PackageItemType.ServiceCategory:
                    {
                        var cat = await _context.ServiceCategories
                            .Where(c => c.Id == a.AssignmentId)
                            .Select(c => new { c.ServiceId, c.Name })
                            .FirstAsync(ct);

                        campServices.Add((a.Id, cat.ServiceId, cat.Name));
                        break;
                    }

                case PackageItemType.ServiceSubcategory:
                    {
                        var sub = await _context.ServiceSubcategories
                            .Where(sc => sc.Id == a.AssignmentId)
                            .Select(sc => new
                            {
                                sc.ServiceCategory.ServiceId,
                                sc.Name
                            })
                            .FirstAsync(ct);

                        campServices.Add((a.Id, sub.ServiceId, sub.Name));
                        break;
                    }

                default:
                    throw new InvalidOperationException($"Unsupported assignment type: {a.AssignmentType}");
            }
        }

        var campServiceIds = campServices.Select(s => s.ServiceId).Distinct().ToList();

        // ==================================================
        // 2. Load IntakeFormResponses (STRICTLY this camp only — avoid cross-camp leakage)
        // ==================================================
        var responseRows = await _context.IntakeFormResponses
            .AsNoTracking()
            .Where(r =>
                r.HealthCampId == campId &&
                campServiceIds.Contains(r.ResolvedServiceId))
            .GroupBy(r => new { r.PatientId, r.ResolvedServiceId })
            .Select(g => new
            {
                g.Key.PatientId,
                g.Key.ResolvedServiceId,
                ServedAt = g.Min(x => x.CreatedAt)
            })
            .ToListAsync(ct);

        var responseLookup = responseRows.ToDictionary(
            x => (x.PatientId, x.ResolvedServiceId),
            x => x.ServedAt);

        // ==================================================
        // 3. Load participant packages
        // ==================================================
        var participantPackages = await _context.HealthCampParticipantPackages
            .AsNoTracking()
            .Where(pp => !pp.IsDeleted && pp.IsActive)
            .Select(pp => new
            {
                pp.ParticipantId,
                pp.HealthCampPackageId,
                pp.HealthCampPackage.ServicePackageId,
                PackageName = pp.HealthCampPackage.ServicePackage.Name
            })
            .ToListAsync(ct);

        var packageLookup = participantPackages.ToDictionary(
            x => x.ParticipantId,
            x => x);

        // ==================================================
        // 4. Resolve package → allowed services (ONCE)
        // ==================================================
        var packageItems = await _context.HealthCampPackageItems
            .AsNoTracking()
            .Where(pi => pi.HealthCampId == campId && pi.ServicePackageId != null)
            .Select(pi => new
            {
                pi.ServicePackageId,
                pi.ReferenceType,
                pi.ReferenceId
            })
            .ToListAsync(ct);

        var packageServiceMap = new Dictionary<Guid, HashSet<Guid>>();

        foreach (var pi in packageItems)
        {
            if (!packageServiceMap.TryGetValue(pi.ServicePackageId!.Value, out var set))
            {
                set = new HashSet<Guid>();
                packageServiceMap[pi.ServicePackageId.Value] = set;
            }

            switch (pi.ReferenceType)
            {
                case PackageItemType.Service:
                    set.Add(pi.ReferenceId);
                    break;

                case PackageItemType.ServiceCategory:
                    set.UnionWith(
                        await _context.ServiceCategories
                            .Where(c => c.Id == pi.ReferenceId)
                            .Select(c => c.ServiceId)
                            .ToListAsync(ct));
                    break;

                case PackageItemType.ServiceSubcategory:
                    set.UnionWith(
                        await _context.ServiceSubcategories
                            .Where(sc => sc.Id == pi.ReferenceId)
                            .Select(sc => sc.ServiceCategory.ServiceId)
                            .ToListAsync(ct));
                    break;
            }
        }

        // ==================================================
        // 5. Base participant query
        // ==================================================
        var baseQuery =
            _context.HealthCampParticipants
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.HealthCamp)
                    .ThenInclude(h => h.Organization)
                .Where(p => p.HealthCampId == campId)
                .Select(p => new
                {
                    Participant = p,
                    PatientId =
                        _context.Patients
                            .Where(pa => pa.UserId == p.UserId && !pa.IsDeleted)
                            .Select(pa => pa.Id)
                            .FirstOrDefault()
                });

        if (participantId.HasValue)
            baseQuery = baseQuery.Where(x => x.Participant.Id == participantId.Value);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            baseQuery = baseQuery.Where(x =>
                EF.Functions.ILike(x.Participant.User.FullName!, $"%{term}%") ||
                EF.Functions.ILike(x.Participant.User.Email!, $"%{term}%") ||
                EF.Functions.ILike(x.Participant.User.Phone!, $"%{term}%"));
        }

        baseQuery = sort?.ToLowerInvariant() switch
        {
            "name" => baseQuery.OrderBy(x => x.Participant.User.FullName),
            "oldest" => baseQuery.OrderBy(x => x.Participant.ParticipatedAt),
            _ => baseQuery.OrderByDescending(x => x.Participant.ParticipatedAt)
        };

        var total = await baseQuery.CountAsync(ct);

        var rawParticipants = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // ==================================================
        // 6. Enrich IN MEMORY — allocation enforced
        // ==================================================
        // ==================================================
        // 6. Enrich IN MEMORY — allocation + response enforced
        // ==================================================
        var items = rawParticipants.Select(x =>
        {
            packageLookup.TryGetValue(x.Participant.Id, out var pkg);

            var allowedServices =
                pkg != null && packageServiceMap.TryGetValue(pkg.ServicePackageId, out var set)
                    ? set
                    : new HashSet<Guid>();

            var completed = campServices
                //  keep camp services, but evaluate allocation PER SERVICE
                .Select(cs =>
                {
                    // service must be allocated to participant
                    if (!allowedServices.Contains(cs.ServiceId))
                        return null;

                    // Only set ServedAt when we have a matching intake form response; otherwise null (not default DateTime).
                    var servedAt = responseLookup.TryGetValue((x.PatientId, cs.ServiceId), out var at) ? at : (DateTime?)null;

                    return new ServiceCompletionDto
                    {
                        ServiceAssignmentId = cs.AssignmentId,
                        ResolvedServiceId = cs.ServiceId,
                        ServiceName = cs.ServiceName,
                        ServedAt = servedAt
                    };
                })
                .Where(s => s != null) // remove non-allocated services
                .ToList()!;

            return new CampParticipantListDto
            {
                Id = x.Participant.Id,
                UserId = x.Participant.UserId,
                PatientId = x.PatientId,
                FullName = x.Participant.User.FullName!,
                Email = x.Participant.User.Email,
                PhoneNumber = x.Participant.User.Phone,
                CompanyName = x.Participant.HealthCamp.Organization.BusinessName!,
                ParticipatedAt = x.Participant.ParticipatedAt,
                Served = null, // camp-wide mode
                CompletedServices = completed,
                PackageId = pkg?.HealthCampPackageId,
                PackageName = pkg?.PackageName
            };
        }).ToList();

        // ==================================================
        // 7. Status filter
        // ==================================================
        items = status switch
        {
            CampParticipantServeStatus.Served =>
                items.Where(p => p.CompletedServices.Any(s => s.ServedAt != null)).ToList(),

            CampParticipantServeStatus.NotServed =>
                items.Where(p => p.CompletedServices.All(s => s.ServedAt == null)).ToList(),

            _ => items
        };

        return new PagedResult<CampParticipantListDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        };
    }



    public async Task<List<HealthCamp>> GetAllUpcomingCampsAsync(CancellationToken ct = default)
    {
        var eat = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
        var nowUtc = DateTime.UtcNow;
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, eat).Date;

        return await _context.HealthCamps
            .Where(c =>
                !c.IsDeleted &&
                (c.CloseDate == null || c.CloseDate > nowUtc) &&
                (
                    c.StartDate >= todayLocal ||
                    (c.IsLaunched &&
                     c.StartDate <= todayLocal &&
                     (c.EndDate ?? c.StartDate) >= todayLocal)
                ))
            .Include(c => c.HealthCampStatus)
            .Include(c => c.Organization)
            .Include(c => c.ServiceAssignments)
            .AsNoTracking()
            .OrderBy(c => c.StartDate)
            .ToListAsync(ct);
    }


    public async Task<List<HealthCamp>> GetAllOngoingCampsAsync(CancellationToken ct = default)
    {
        var eat = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
        var nowUtc = DateTime.UtcNow;
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, eat).Date;

        return await _context.HealthCamps
            .Where(c =>
                !c.IsDeleted &&
                (c.CloseDate == null || c.CloseDate > nowUtc) && c.HealthCampStatus.Name == "Ongoing" &&
                (
                    c.StartDate >= todayLocal ||
                    (c.IsLaunched &&

                     (c.EndDate ?? c.StartDate) >= todayLocal)
                ))
            .Include(c => c.HealthCampStatus)
            .Include(c => c.Organization)
            .Include(c => c.ServiceAssignments)
            .AsNoTracking()
            .OrderBy(c => c.StartDate)
            .ToListAsync(ct);
    }



    public async Task<List<HealthCamp>> GetAllCompleteCampsAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.HealthCamps
            .Where(c => c.IsLaunched
                        && ((c.EndDate ?? c.StartDate) < today))
            .Include(c => c.Organization)
            .AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);
    }

    public async Task<List<HealthCamp>> GetAllCanceledCampsAsync(CancellationToken ct = default)
    {
        return await _context.HealthCamps
            .Where(c => !c.IsLaunched
                        || (c.HealthCampStatus != null &&
                            EF.Functions.ILike(c.HealthCampStatus.Name.ToLowerInvariant(), "suspended")))
            .Include(c => c.Organization)
            .AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);
    }


    public async Task<List<HealthCampWithRolesDto>> GetMyCampsWithRolesByStatusAsync(
    Guid? subcontractorId,
    string status,
    CancellationToken ct = default)
    {
        Console.WriteLine("─────────────────────────────────────────────");
        Console.WriteLine("GetMyCampsWithRolesByStatusAsync START");
        Console.WriteLine($"Input subcontractorId: {subcontractorId}");
        Console.WriteLine($"Input status: {status}");
        Console.WriteLine("─────────────────────────────────────────────");

        var today = DateTime.UtcNow.Date;
        Console.WriteLine($"Today (UTC Date): {today}");

        // ─────────────────────────────────────────────
        // 1. Base query (NO subcontractor filter yet)
        // ─────────────────────────────────────────────
        Console.WriteLine("STEP 1: Building base query (HealthCampServiceAssignments)");

        var baseQuery = _context.HealthCampServiceAssignments
            .Include(x => x.HealthCamp)
                .ThenInclude(c => c.Organization)
            .Include(x => x.HealthCamp)
                .ThenInclude(c => c.HealthCampStatus)
            .Include(x => x.Role)
            .Where(x => x.HealthCamp.IsActive);

        Console.WriteLine("Base query created (IsActive = true)");

        // ─────────────────────────────────────────────
        // 2. Apply subcontractor filter ONLY if present
        // ─────────────────────────────────────────────
        if (subcontractorId.HasValue)
        {
            Console.WriteLine($"STEP 2: Applying subcontractor filter: {subcontractorId.Value}");
            baseQuery = baseQuery.Where(x => x.SubcontractorId == subcontractorId.Value);
        }
        else
        {
            Console.WriteLine("STEP 2: No subcontractor filter applied (privileged view)");
        }

        // ─────────────────────────────────────────────
        // 3. Status filtering
        // ─────────────────────────────────────────────
        Console.WriteLine($"STEP 3: Applying status filter: {status}");

        baseQuery = status.ToLowerInvariant() switch
        {
            "upcoming" => baseQuery.Where(x =>
                x.HealthCamp.IsLaunched &&
                (x.HealthCamp.EndDate ?? x.HealthCamp.StartDate) >= today),

            "complete" => baseQuery.Where(x =>
                x.HealthCamp.IsLaunched &&
                ((x.HealthCamp.EndDate ?? x.HealthCamp.StartDate) < today)),

            "canceled" => baseQuery.Where(x =>
                !x.HealthCamp.IsLaunched ||
                (x.HealthCamp.HealthCampStatus != null &&
                 x.HealthCamp.HealthCampStatus.Name == HealthCampStatusNames.Suspended)),

            _ => baseQuery
        };

        Console.WriteLine("Status filter applied");

        // ─────────────────────────────────────────────
        // 4. Materialize
        // ─────────────────────────────────────────────
        Console.WriteLine("STEP 4: Executing query (materializing assignments)");

        var assignments = await baseQuery
            .AsNoTracking()
            .ToListAsync(ct);

        Console.WriteLine($"Assignments fetched: {assignments.Count}");

        foreach (var a in assignments)
        {
            Console.WriteLine(
                $"  AssignmentId={a.AssignmentId}, " +
                $"Type={a.AssignmentType}, " +
                $"CampId={a.HealthCampId}, " +
                $"SubcontractorId={a.SubcontractorId}"
            );
        }

        // ─────────────────────────────────────────────
        // 5. Normalize subcategory → category
        // ─────────────────────────────────────────────
        Console.WriteLine("STEP 5: Normalizing subcategories → categories");

        var normalized = new List<(Guid RefId, PackageItemType Type, HealthCampServiceAssignment Source)>();

        foreach (var a in assignments)
        {
            if (a.AssignmentType == PackageItemType.ServiceSubcategory)
            {
                Console.WriteLine($"  Resolving parent category for subcategory {a.AssignmentId}");

                var parent = await _context.ServiceSubcategories
                    .Where(sc => sc.Id == a.AssignmentId)
                    .Select(sc => sc.ServiceCategory)
                    .FirstOrDefaultAsync(ct);

                if (parent != null)
                {
                    Console.WriteLine($"    → Parent category resolved: {parent.Id}");
                    normalized.Add((parent.Id, PackageItemType.ServiceCategory, a));
                    continue;
                }

                Console.WriteLine("    → No parent category found");
            }

            normalized.Add((a.AssignmentId, a.AssignmentType, a));
        }

        Console.WriteLine($"Normalized count: {normalized.Count}");

        // ─────────────────────────────────────────────
        // 6. Deduplicate (prefer category)
        // ─────────────────────────────────────────────
        Console.WriteLine("STEP 6: Deduplicating assignments");

        var finalAssignments = normalized
            .GroupBy(x => x.RefId)
            .Select(g =>
            {
                var category = g.FirstOrDefault(x => x.Type == PackageItemType.ServiceCategory);
                return category.RefId != Guid.Empty ? category : g.First();
            })
            .ToList();

        Console.WriteLine($"Final assignment count after dedupe: {finalAssignments.Count}");

        // ─────────────────────────────────────────────
        // 7. Resolve + project
        // ─────────────────────────────────────────────
        Console.WriteLine("STEP 7: Resolving names and projecting DTOs");

        var resolver = new PackageReferenceResolverService(
            _serviceRepo, _categoryRepo, _subcategoryRepo);

        var result = new List<HealthCampWithRolesDto>();

        foreach (var campGroup in finalAssignments.GroupBy(x => x.Source.HealthCamp))
        {
            Console.WriteLine($"Processing camp: {campGroup.Key.Id}");

            var dto = new HealthCampWithRolesDto
            {
                CampId = campGroup.Key.Id,
                ClientName = campGroup.Key.Organization?.BusinessName ?? "—",
                Venue = campGroup.Key.Location ?? "—",
                StartDate = campGroup.Key.StartDate,
                EndDate = campGroup.Key.EndDate,
                Status = campGroup.Key.HealthCampStatus?.Name ?? "Unknown",
                Roles = new List<RoleAssignmentDto>()
            };

            foreach (var boothGroup in campGroup.GroupBy(x => new { x.RefId, x.Type }))
            {
                var boothName = await resolver.GetNameAsync(
                    boothGroup.Key.Type,
                    boothGroup.Key.RefId);

                Console.WriteLine(
                    $"  Booth resolved: {boothName} " +
                    $"(RefId={boothGroup.Key.RefId}, Type={boothGroup.Key.Type})"
                );

                var roles = boothGroup
                    .Select(x => x.Source.Role?.Name ?? "—")
                    .Distinct();

                foreach (var role in roles)
                {
                    Console.WriteLine($"    Role added: {role}");

                    dto.Roles.Add(new RoleAssignmentDto
                    {
                        AssignedBooth = boothName,
                        AssignedRole = role,
                        ServiceId = boothGroup.Key.RefId
                    });
                }
            }

            result.Add(dto);
        }

        Console.WriteLine($"STEP 8: Final DTO count: {result.Count}");
        Console.WriteLine("GetMyCampsWithRolesByStatusAsync END");
        Console.WriteLine("─────────────────────────────────────────────");

        return result;
    }

    public async Task<List<HealthCampPatientDto>> GetCampPatientsByStatusAsync(
    Guid campId,
    string filter,
    string? q,
    string? sort,
    int page,
    int pageSize,
    CancellationToken ct = default)
    {
        // Start with safe includes
        var query = _context.HealthCampParticipants
            .Where(p => p.HealthCampId == campId)
            .Include(p => p.User)
            .Include(p => p.HealthCamp)
                .ThenInclude(h => h.Organization)
            .Include(p => p.HealthAssessments) // include to avoid Any() failure
            .AsSplitQuery(); // optional: avoid Cartesian explosion

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                EF.Functions.ILike(p.User.FullName, $"%{term}%") ||
                EF.Functions.ILike(p.User.Email, $"%{term}%") ||
                (p.User.Phone != null && EF.Functions.ILike(p.User.Phone, $"%{term}%")));
        }

        // Materialize query first (EF cannot translate .Any() + .Select reliably)
        var list = await query.ToListAsync(ct);

        // Populate PatientId manually
        var userIds = list.Select(p => p.UserId).Distinct().ToList();

        var patientMap = await _context.Patients
            .Where(x => userIds.Contains(x.UserId))
            .Select(x => new { x.UserId, x.Id })
            .ToDictionaryAsync(x => x.UserId, x => x.Id, ct);

        foreach (var p in list)
        {
            if (patientMap.TryGetValue(p.UserId, out var pid))
            {
                p.PatientId = pid;
            }
        }



        // In-memory filtering
        list = filter.ToLowerInvariant() switch
        {
            "served" => list.Where(p => p.ParticipatedAt != null || p.HealthAssessments.Any()).ToList(),
            "not-seen" => list.Where(p => p.ParticipatedAt == null && !p.HealthAssessments.Any()).ToList(),
            _ => list
        };

        // Sorting
        list = (sort ?? string.Empty).ToLowerInvariant() switch
        {
            "oldest" => list.OrderBy(p => p.CreatedAt).ToList(),
            _ => list.OrderByDescending(p => p.CreatedAt).ToList()
        };

        // Pagination + projection
        return list
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new HealthCampPatientDto
            {
                PatientId = p.PatientId?.ToString() ?? "—",
                FullName = p.User.FullName!,
                Company = p.HealthCamp.Organization?.BusinessName ?? "—",
                PhoneNumber = p.User.Phone ?? "",
                Email = p.User.Email ?? ""
            })
            .ToList();
    }


    public async Task<CampPatientDetailWithFormsDto?> GetCampPatientDetailWithFormsAsync(
    Guid campId,
    Guid participantId,
    Guid? subcontractorId,
    CancellationToken ct = default)
    {
        // STEP 1: Load participant + camp + org + user
        var p = await _context.HealthCampParticipants
            .Where(x => x.Id == participantId && x.HealthCampId == campId)
            .Select(x => new
            {
                Participant = x,
                Camp = x.HealthCamp,
                OrgName = x.HealthCamp.Organization.BusinessName,
                Venue = x.HealthCamp.Location,
                Status = x.HealthCamp.HealthCampStatus != null ? x.HealthCamp.HealthCampStatus.Name : "Unknown",
                User = x.User
            })
            .AsNoTracking()
            .FirstOrDefaultAsync(ct);

        if (p == null)
            return null;

        // STEP 2: Lookup patient
        var patient = await _context.Patients
            .AsNoTracking()
            .FirstOrDefaultAsync(pa => pa.UserId == p.User.Id && !pa.IsDeleted, ct);

        if (patient == null)
            return null;

        // STEP 3: Served check (now uses ParticipantServiceStatuses)
        var served = await _context.HealthCampParticipantServiceStatuses
            .AnyAsync(s => s.ParticipantId == participantId && s.ServedAt != null, ct);

        // STEP 4: Load assignments polymorphically
        IQueryable<HealthCampServiceAssignment> assignQ = _context.HealthCampServiceAssignments
            .Where(a => a.HealthCampId == campId)
            .Include(a => a.Role);

        if (subcontractorId.HasValue)
            assignQ = assignQ.Where(a => a.SubcontractorId == subcontractorId.Value);

        var assignments = await assignQ.AsNoTracking().ToListAsync(ct);

        // STEP 5: Final response
        var dto = new CampPatientDetailWithFormsDto
        {
            ParticipantId = p.Participant.Id,
            UserId = p.User.Id,
            PatientCode = patient.Id.ToString(),
            FullName = p.User.FullName ?? "Patient",
            Email = p.User.Email,
            Phone = p.User.Phone,
            CampId = p.Camp.Id,
            ClientName = p.OrgName,
            Venue = p.Venue ?? "",
            StartDate = p.Camp.StartDate,
            EndDate = p.Camp.EndDate,
            Status = p.Status,
            Served = served // ✅ Now reflects actual service completion
        };

        // STEP 6: Normalize assignments
        var normalizedAssignments = new List<(Guid RefId, PackageItemType Type, HealthCampServiceAssignment Source)>();

        foreach (var a in assignments)
        {
            if (a.AssignmentType == PackageItemType.ServiceSubcategory)
            {
                var parent = await _context.ServiceSubcategories
                    .Where(sc => sc.Id == a.AssignmentId)
                    .Select(sc => sc.ServiceCategory)
                    .FirstOrDefaultAsync(ct);

                if (parent != null)
                {
                    normalizedAssignments.Add((parent.Id, PackageItemType.ServiceCategory, a));
                    continue;
                }
            }

            normalizedAssignments.Add((a.AssignmentId, a.AssignmentType, a));
        }

        // STEP 7: Deduplicate
        var deduped = normalizedAssignments
            .GroupBy(x => new { x.RefId, x.Type })
            .Select(g => g.First())
            .ToList();

        var finalAssignments = deduped
            .GroupBy(x => x.RefId)
            .Select(g =>
            {
                var category = g.FirstOrDefault(x => x.Type == PackageItemType.ServiceCategory);
                return category.Source != null ? category : g.First();
            })
            .ToList();

        // STEP 8: Build DTO list
        var result = new List<AssignedServiceWithFormDto>();
        var referenceResolver = new PackageReferenceResolverService(_serviceRepo, _categoryRepo, _subcategoryRepo);

        foreach (var item in finalAssignments)
        {
            var any = item.Source;
            IntakeForm? form = null;
            string displayName = await referenceResolver.GetNameAsync(item.Type, item.RefId);

            switch (item.Type)
            {
                case PackageItemType.Service:
                    form = await _context.Services
                        .Where(s => s.Id == item.RefId)
                        .Include(s => s.IntakeForm).ThenInclude(f => f.Versions)
                        .Include(s => s.IntakeForm).ThenInclude(f => f.Sections)
                            .ThenInclude(sec => sec.Fields).ThenInclude(ff => ff.Options)
                        .Select(s => s.IntakeForm)
                        .FirstOrDefaultAsync(ct);
                    break;

                case PackageItemType.ServiceCategory:
                    form = await _context.ServiceCategories
                        .Where(c => c.Id == item.RefId)
                        .Include(c => c.IntakeForm).ThenInclude(f => f.Versions)
                        .Include(c => c.IntakeForm).ThenInclude(f => f.Sections)
                            .ThenInclude(sec => sec.Fields).ThenInclude(ff => ff.Options)
                        .Select(c => c.IntakeForm)
                        .FirstOrDefaultAsync(ct);
                    break;
            }

            result.Add(new AssignedServiceWithFormDto
            {
                ServiceId = item.RefId,
                ServiceName = displayName,
                ProfessionId = any.ProfessionId,
                AssignedRole = any.Role?.Name,
                Form = MapForm(form)
            });
        }

        dto.Assignments = [.. result.OrderBy(x => x.ServiceName)];
        return dto;
    }



    // --- helpers ---

    private static FormResponseDto? MapForm(IntakeForm? f)
    {
        if (f == null) return null;

        // If the form is sectioned
        if (f.Sections != null && f.Sections.Count > 0)
        {
            return new FormResponseDto
            {
                Id = f.Id,
                Name = f.Name,
                Description = f.Description,
                IntakeFormVersionId = f.Versions.OrderByDescending(v => v.CreatedAt).FirstOrDefault()?.Id,
                Sections = f.Sections
                    .OrderBy(s => s.Order)
                    .Select(s => new FormSectionResponseDto
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Description = s.Description,
                        Order = s.Order,
                        Fields = s.Fields
                            .OrderBy(ff => ff.Order)
                            .Select(MapField)
                            .ToList()
                    })
                    .ToList(),
                Fields = new List<FormFieldResponseDto>() // empty when sectioned
            };
        }

        // Unsectioned (flat) form
        return new FormResponseDto
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description,
            Sections = new List<FormSectionResponseDto>(),
            Fields = f.Fields != null
                ? f.Fields.OrderBy(ff => ff.Order).Select(MapField).ToList()
                : new List<FormFieldResponseDto>()
        };
    }

    private static FormFieldResponseDto MapField(IntakeFormField ff)
    {
        return new FormFieldResponseDto
        {
            Id = ff.Id,
            FormId = ff.FormId,
            SectionId = ff.SectionId,
            Label = ff.Label,
            FieldType = ff.FieldType,
            IsRequired = ff.IsRequired,
            Order = ff.Order,
            HasConditionalLogic = ff.HasConditionalLogic,
            ConditionalLogicType = ff.ConditionalLogicType,
            TriggerFieldId = ff.TriggerFieldId,
            TriggerValueOptionId = ff.TriggerValueOptionId,
            ValidationType = ff.ValidationType,
            ValidationPattern = ff.ValidationPattern,
            MinValue = ff.MinValue,
            MaxValue = ff.MaxValue,
            MinLength = ff.MinLength,
            MaxLength = ff.MaxLength,
            CustomErrorMessage = ff.CustomErrorMessage,
            LayoutPosition = ff.LayoutPosition,
            Options = (ff.Options ?? new List<IntakeFormFieldOption>())
                .Select(o => new FieldOptionResponseDto
                {
                    Id = o.Id,
                    Value = o.Value
                    // DisplayText = o.DisplayText
                })
                .ToList()
        };
    }

    public async Task<List<OrganizationCampListDto>> GetCampsByOrganizationAsync(Guid organizationId, CancellationToken ct = default)
    {
        return await _context.HealthCamps
            .Where(c => c.OrganizationId == organizationId)
            .Select(c => new OrganizationCampListDto
            {
                Id = c.Id,
                CampName = c.Name,
                CampDate = c.StartDate,
                CampVenue = c.Location ?? "Not specified",
                CampStatus = c.HealthCampStatus != null ? c.HealthCampStatus.Name : "Unknown",
                NumberOfPatients = c.Participants.Count()
            })
            .OrderByDescending(c => c.CampDate)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task<OrganizationStatsDto> GetOrganizationStatsAsync(Guid organizationId, CancellationToken ct = default)
    {
        var expectedPatients = await _context.HealthCampParticipants
            .Where(p => p.HealthCamp.OrganizationId == organizationId)
            .Select(p => p.PatientId ?? p.UserId)
            .Distinct()
            .CountAsync(ct);

        var campsHeld = await _context.HealthCamps
            .Where(c => c.OrganizationId == organizationId &&
                       (c.IsLaunched ||
                        (c.HealthCampStatus != null &&
                         (c.HealthCampStatus.Name == "Completed" || c.HealthCampStatus.Name == "Active"))))
            .CountAsync(ct);

        return new OrganizationStatsDto
        {
            ExpectedPatients = expectedPatients,
            CampsHeld = campsHeld
        };
    }

    public async Task<List<DateTime>> GetUpcomingCampDatesAsync(CancellationToken ct = default)
    {
        var eat = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
        var nowUtc = DateTime.UtcNow;
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, eat).Date;

        return await _context.HealthCamps
            .Where(c =>
                !c.IsDeleted &&
                (c.CloseDate == null || c.CloseDate > nowUtc) &&
                (
                    c.StartDate >= todayLocal ||
                    (c.IsLaunched &&
                     c.StartDate <= todayLocal &&
                     (c.EndDate ?? c.StartDate) >= todayLocal)
                ))
            .Select(c => c.StartDate)
            .Distinct()
            .OrderBy(date => date)
            .ToListAsync(ct);
    }

    // MultiCamp batch fetching

    public async Task<List<HealthCamp>> GetAllWithDetailsAsync(CancellationToken ct = default)
    {
        return await _context.HealthCamps
            .AsNoTracking()
            .Include(h => h.Organization)
            .Include(h => h.HealthCampStatus)
            .Where(h => !h.IsDeleted)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<Guid, List<HealthCampParticipant>>> GetParticipantsForMultipleCampsAsync(
        List<Guid> campIds,
        CancellationToken ct = default)
    {
        var allParticipants = await _context.HealthCampParticipants
            .AsNoTracking()
            .Where(p => campIds.Contains(p.HealthCampId) && !p.IsDeleted)
            .ToListAsync(ct);

        return allParticipants
            .GroupBy(p => p.HealthCampId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

}
