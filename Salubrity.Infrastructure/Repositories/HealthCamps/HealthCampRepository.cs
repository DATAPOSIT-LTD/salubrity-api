using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Forms;
using Salubrity.Application.DTOs.HealthCamps;
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


    private IQueryable<HealthCamp> CampsForSubcontractor(Guid subcontractorId)
    {
        return _context.HealthCampServiceAssignments
            .Where(a => (a.SubcontractorId == subcontractorId) && !a.HealthCamp.IsDeleted)
            .Select(a => a.HealthCamp)
            .Distinct()
            .Include(c => c.Organization)
            .AsNoTracking()
            .AsSplitQuery();
    }
    public async Task<List<HealthCamp>> GetMyUpcomingCampsAsync(Guid subcontractorId, CancellationToken ct = default)
    {
        var eat = TimeZoneInfo.FindSystemTimeZoneById("Africa/Nairobi");
        var nowUtc = DateTime.UtcNow;
        var todayLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, eat).Date;

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
        // --- Step 1: Compute total assignments only for camp-wide mode
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

                // ✅ If station-specific: check this serviceAssignmentId only
                Served = serviceAssignmentId != null
                    ? _context.HealthCampParticipantServiceStatuses
                        .Any(s =>
                            s.ParticipantId == p.Id &&
                            s.ServiceAssignmentId == serviceAssignmentId &&
                            s.ServedAt != null)
                    // ✅ Otherwise, camp-wide: all assigned services must be served
                    : _context.HealthCampParticipantServiceStatuses
                        .Count(s => s.ParticipantId == p.Id && s.ServedAt != null) >= totalAssignments
            };

        // --- Search ---
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                (x.FullName != null && EF.Functions.ILike(x.FullName, $"%{term}%")) ||
                (x.Email != null && EF.Functions.ILike(x.Email, $"%{term}%")) ||
                (x.PhoneNumber != null && EF.Functions.ILike(x.PhoneNumber, $"%{term}%")));
        }

        // --- Sort ---
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


    public async Task<List<CampParticipantListDto>> GetCampParticipantsAllAsync(
       Guid campId,
       Guid? serviceAssignmentId,
       string? q,
       string? sort,
       int page,
       int pageSize,
       CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var query = BaseParticipantsDto(campId, q, sort);

        // If a station/serviceAssignmentId is provided, filter participants
        if (serviceAssignmentId.HasValue)
        {
            query = query.Where(p =>
                _context.HealthCampParticipantServiceStatuses.Any(s =>
                    s.ParticipantId == p.Id &&
                    s.ServiceAssignmentId == serviceAssignmentId &&
                    s.ServedAt != null));
        }

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
    }
    public async Task<List<CampParticipantListDto>> GetCampParticipantsServedAsync(
        Guid campId,
        Guid? serviceAssignmentOrServiceId,
        string? q,
        string? sort,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        // 🧠 Step 1: Resolve correct ServiceAssignmentId if a ServiceId was passed
        Guid? resolvedAssignmentId = null;

        if (serviceAssignmentOrServiceId.HasValue)
        {
            // Try to find a matching assignment for this camp using the passed ID
            resolvedAssignmentId = await _context.HealthCampServiceAssignments
                .Where(a => a.HealthCampId == campId &&
                            (a.Id == serviceAssignmentOrServiceId.Value || a.AssignmentId == serviceAssignmentOrServiceId.Value))
                .Select(a => a.Id)
                .FirstOrDefaultAsync(ct);

            if (resolvedAssignmentId == Guid.Empty)
                resolvedAssignmentId = null; // no match found, treat as no filter
        }

        // 🧮 Step 2: Compute total assignments in this camp (used for "camp-wide served")
        var totalAssignments = await _context.HealthCampServiceAssignments
            .Where(a => a.HealthCampId == campId && !a.IsDeleted)
            .CountAsync(ct);

        // 🧩 Step 3: Build query
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

                // List of completed services
                CompletedServices = _context.HealthCampParticipantServiceStatuses
                    .Where(s => s.ParticipantId == p.Id && s.ServedAt != null)
                    .Select(s => new ServiceCompletionDto
                    {
                        ServiceAssignmentId = s.ServiceAssignmentId,
                        ServiceName =
                            s.ServiceAssignment.AssignmentType == PackageItemType.Service
                                ? _context.Services
                                    .Where(sv => sv.Id == s.ServiceAssignment.AssignmentId)
                                    .Select(sv => sv.Name)
                                    .FirstOrDefault()
                                : _context.ServiceCategories
                                    .Where(sc => sc.Id == s.ServiceAssignment.AssignmentId)
                                    .Select(sc => sc.Name)
                                    .FirstOrDefault(),
                        ServedAt = s.ServedAt
                    })
                    .ToList(),

                // ✅ Participant served logic (station or full camp)
                Served = resolvedAssignmentId.HasValue
                    ? _context.HealthCampParticipantServiceStatuses
                        .Any(s =>
                            s.ParticipantId == p.Id &&
                            s.ServiceAssignmentId == resolvedAssignmentId.Value &&
                            s.ServedAt != null)
                    : _context.HealthCampParticipantServiceStatuses
                        .Count(s => s.ParticipantId == p.Id && s.ServedAt != null) >= totalAssignments
            };

        // 🔍 Step 4: Search
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                (x.FullName != null && EF.Functions.ILike(x.FullName, $"%{term}%")) ||
                (x.Email != null && EF.Functions.ILike(x.Email, $"%{term}%")) ||
                (x.PhoneNumber != null && EF.Functions.ILike(x.PhoneNumber, $"%{term}%")));
        }

        // ↕ Step 5: Sorting
        var s = sort?.ToLowerInvariant();
        query = s switch
        {
            "name" => query.OrderBy(x => x.FullName),
            "oldest" => query.OrderBy(x => x.ParticipatedAt == null)
                             .ThenBy(x => x.ParticipatedAt),
            _ => query.OrderByDescending(x => x.ParticipatedAt != null)
                             .ThenByDescending(x => x.ParticipatedAt)
        };

        // 📄 Step 6: Pagination
        var result = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);

        return result;
    }




    public async Task<List<CampParticipantListDto>> GetCampParticipantsNotSeenAsync(
        Guid campId,
        Guid? serviceAssignmentId,
        string? q,
        string? sort,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 20;

        var query =
            from p in _context.HealthCampParticipants
            where p.HealthCampId == campId
                  && !_context.HealthCampParticipantServiceStatuses
                        .Any(s =>
                            s.ParticipantId == p.Id &&
                            (serviceAssignmentId == null || s.ServiceAssignmentId == serviceAssignmentId) &&
                            s.ServedAt != null)
            select new CampParticipantListDto
            {
                Id = p.Id,
                UserId = p.UserId,
                PatientId = _context.Patients
                    .Where(pa => pa.UserId == p.UserId && !pa.IsDeleted)
                    .Select(pa => (Guid?)pa.Id)
                    .FirstOrDefault(),
                FullName = p.User.FullName!,
                Email = p.User.Email,
                PhoneNumber = p.User.Phone,
                CompanyName = p.HealthCamp.Organization.BusinessName!,
                ParticipatedAt = p.ParticipatedAt,
                Served = false
            };

        // 🔍 Search
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(x =>
                (x.FullName != null && EF.Functions.ILike(x.FullName, $"%{term}%")) ||
                (x.Email != null && EF.Functions.ILike(x.Email, $"%{term}%")) ||
                (x.PhoneNumber != null && EF.Functions.ILike(x.PhoneNumber, $"%{term}%")));
        }

        // ↕ Sort
        var s = sort?.ToLowerInvariant();
        query = s switch
        {
            "name" => query.OrderBy(x => x.FullName),
            "oldest" => query.OrderBy(x => x.ParticipatedAt == null)
                             .ThenBy(x => x.ParticipatedAt),
            _ => query.OrderByDescending(x => x.ParticipatedAt != null)
                      .ThenByDescending(x => x.ParticipatedAt)
        };

        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(ct);
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
        Guid subcontractorId,
        string status,
        CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        var baseQuery = _context.HealthCampServiceAssignments
            .Where(x => x.SubcontractorId == subcontractorId)
            .Include(x => x.HealthCamp)
                .ThenInclude(c => c.Organization)
            .Include(x => x.HealthCamp)
                .ThenInclude(c => c.HealthCampStatus)
            .Include(x => x.Role)
            .Where(x => x.HealthCamp.IsActive);

        baseQuery = status.ToLowerInvariant() switch
        {
            "upcoming" => baseQuery.Where(x =>
                x.HealthCamp.IsLaunched &&
                ((x.HealthCamp.EndDate ?? x.HealthCamp.StartDate) >= today) &&
                (x.HealthCamp.CloseDate == null || x.HealthCamp.CloseDate >= today)),

            "complete" => baseQuery.Where(x =>
                x.HealthCamp.IsLaunched &&
                ((x.HealthCamp.EndDate ?? x.HealthCamp.StartDate) < today)),

            "canceled" => baseQuery.Where(x =>
                !x.HealthCamp.IsLaunched ||
                (x.HealthCamp.HealthCampStatus != null &&
                 x.HealthCamp.HealthCampStatus.Name == HealthCampStatusNames.Suspended)),

            _ => baseQuery
        };

        // Materialize
        var assignments = await baseQuery.AsNoTracking().ToListAsync(ct);

        // Normalize subcategory → category
        var normalized = new List<(Guid RefId, PackageItemType Type, HealthCampServiceAssignment Source)>();
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
                    normalized.Add((parent.Id, PackageItemType.ServiceCategory, a));
                    continue;
                }
            }

            normalized.Add((a.AssignmentId, a.AssignmentType, a));
        }

        // Deduplicate: prefer category over subcategory
        var finalAssignments = normalized
            .GroupBy(x => x.RefId)
            .Select(g =>
            {
                var category = g.FirstOrDefault(x => x.Type == PackageItemType.ServiceCategory);
                return category.RefId != Guid.Empty ? category : g.First();
            })
            .ToList();

        // Resolver
        var resolver = new PackageReferenceResolverService(_serviceRepo, _categoryRepo, _subcategoryRepo);

        var result = new List<HealthCampWithRolesDto>();

        foreach (var campGroup in finalAssignments.GroupBy(x => x.Source.HealthCamp))
        {
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

            // Group by booth name only
            foreach (var boothGroup in campGroup.GroupBy(x => new { x.RefId, x.Type }))
            {
                var any = boothGroup.First();
                var boothName = await resolver.GetNameAsync(boothGroup.Key.Type, boothGroup.Key.RefId);

                // Collect all roles for this booth
                var roles = boothGroup
                    .Select(x => x.Source.Role?.Name ?? "—")
                    .Distinct()
                    .ToList();

                foreach (var role in roles)
                {
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

        // STEP 3: ✅ Served check (now uses ParticipantServiceStatuses)
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


}
