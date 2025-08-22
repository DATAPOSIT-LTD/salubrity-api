using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Join;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Infrastructure.Repositories.HealthCamps;

public class HealthCampRepository : IHealthCampRepository
{
    private readonly AppDbContext _context;
    private readonly IPackageReferenceResolver _referenceResolver;
    private readonly IMapper _mapper;

    public HealthCampRepository(AppDbContext context, IPackageReferenceResolver referenceResolver, IMapper mapper)
    {
        _context = context;
        _referenceResolver = referenceResolver;
        _mapper = mapper;
    }

    public async Task<List<HealthCampListDto>> GetAllAsync()
    {
        var camps = await _context.HealthCamps
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .Include(c => c.Organization)
            .Include(c => c.ServicePackage)     //  load chosen package
            .Include(c => c.ServiceAssignments) // for counts/status
            .Include(c => c.HealthCampStatus)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var list = new List<HealthCampListDto>(camps.Count);

        foreach (var c in camps)
        {
            list.Add(new HealthCampListDto
            {
                Id = c.Id,
                ClientName = c.Organization?.BusinessName ?? "N/A",
                ExpectedPatients = c.ExpectedParticipants ?? 0,
                Venue = c.Location ?? "N/A",
                DateRange = $"{c.StartDate:dd} - {c.EndDate:dd MMM, yyyy}",
                SubcontractorCount = c.ServiceAssignments?.Count ?? 0,
                Status = c.HealthCampStatus?.Name ?? "Unknown",
                PackageName = c.ServicePackage?.Name ?? "N/A"   //  chosen package only
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
            .Include(c => c.ServicePackage)   // <-- chosen package
            .Include(c => c.PackageItems)     // <-- items (services/categories)
            .Include(c => c.ServiceAssignments)
            .Include(c => c.HealthCampStatus)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

        if (camp is null) return null;

        var dto = new HealthCampDetailDto
        {
            Id = camp.Id,
            Name = camp.Name,
            StartDate = camp.StartDate,
            ClientName = camp.Organization?.BusinessName ?? "N/A",
            Venue = camp.Location ?? "N/A",
            ExpectedPatients = camp.ExpectedParticipants ?? 0,
            SubcontractorCount = camp.ServiceAssignments?.Count ?? 0,
            // chosen package only
            PackageName = camp.ServicePackage?.Name ?? "N/A",
            PackageCost = camp.ServicePackage?.Price,
            Status = camp.HealthCampStatus?.Name ?? "Unknown",
            InsurerName = camp.Organization?.InsuranceProviders?.FirstOrDefault()?.InsuranceProvider?.Name ?? string.Empty,
            ServiceStations = new List<ServiceStationDto>()
        };

        // Resolve station names (services/categories) concurrently
        var itemTasks = camp.PackageItems
            .Where(pi => !pi.IsDeleted)
            .Select(async pi =>
            {
                var name = await _referenceResolver.GetNameAsync(pi.ReferenceType, pi.ReferenceId);
                return new ServiceStationDto
                {
                    Id = pi.Id,
                    Name = name,
                    PatientsServed = 24,     // TODO: replace with real metrics
                    PendingService = 35,     // TODO: replace with real metrics
                    AvgTimePerPatient = "3 min",
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
            .Where(c => !c.IsLaunched || c.HealthCampStatus!.Name == "Canceled")
            .Include(c => c.Organization)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<HealthCampListDto>>(items);
    }


    private IQueryable<HealthCamp> CampsForSubcontractor(Guid subcontractorId)
    {
        return _context.HealthCampServiceAssignments
            .Where(a => a.SubcontractorId == subcontractorId)
            .Select(a => a.HealthCamp)
            .Distinct()
            .Include(c => c.Organization)
            .AsNoTracking()
            .AsSplitQuery();
    }

    public async Task<List<HealthCamp>> GetMyUpcomingCampsAsync(Guid subcontractorId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        // “Upcoming” tab includes ongoing + future
        // Use (EndDate ?? StartDate) >= today to include ongoing (spanning) and single-day
        return await CampsForSubcontractor(subcontractorId)
            .Where(c => c.IsLaunched
                        && ((c.EndDate ?? c.StartDate) >= today) && c.CloseDate <= today)
            .OrderBy(c => c.StartDate)
            .ToListAsync(ct);
    }

    public async Task<List<HealthCamp>> GetMyCompleteCampsAsync(Guid subcontractorId, CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        return await CampsForSubcontractor(subcontractorId)
            .Where(c => c.IsLaunched
                        && ((c.EndDate ?? c.StartDate) < today))
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);
    }

    public async Task<List<HealthCamp>> GetMyCanceledCampsAsync(Guid subcontractorId, CancellationToken ct = default)
    {
        // Robust canceled check: either not launched OR status labeled “Canceled”
        return await CampsForSubcontractor(subcontractorId)
            .Where(c => !c.IsLaunched
                        || (c.HealthCampStatus != null &&
                            EF.Functions.ILike(c.HealthCampStatus.Name.ToLowerInvariant(), "canceled")))
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);
    }

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
            _ => query.OrderByDescending(p => p.CreatedAt) // newest default
        };

        return query.AsNoTracking();
    }

    private static IQueryable<CampParticipantListDto> Project(IQueryable<Domain.Entities.Join.HealthCampParticipant> q)
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
            // Served = checked in UI: mark served if participated OR has any assessments
            Served = p.ParticipatedAt != null || p.HealthAssessments.Any()
        });
    }

    public async Task<List<CampParticipantListDto>> GetCampParticipantsAllAsync(Guid campId, string? q, string? sort, int page, int pageSize, CancellationToken ct = default)
    {
        return await Project((IQueryable<Domain.Entities.Join.HealthCampParticipant>)BaseParticipants(campId, q, sort))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<CampParticipantListDto>> GetCampParticipantsServedAsync(Guid campId, string? q, string? sort, int page, int pageSize, CancellationToken ct = default)
    {
        return await Project(BaseParticipants(campId, q, sort)
                .Where(p => p.ParticipatedAt != null || p.HealthAssessments.Any()))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<List<CampParticipantListDto>> GetCampParticipantsNotSeenAsync(Guid campId, string? q, string? sort, int page, int pageSize, CancellationToken ct = default)
    {
        return await Project(BaseParticipants(campId, q, sort)
                .Where(p => p.ParticipatedAt == null && !p.HealthAssessments.Any()))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }


    public async Task<List<HealthCamp>> GetAllUpcomingCampsAsync(CancellationToken ct = default)
    {
        var today = DateTime.UtcNow.Date;

        return await _context.HealthCamps
            .Where(c => c.IsLaunched
                        && ((c.EndDate ?? c.StartDate) >= today)
                        && c.CloseDate <= today)
            .Include(c => c.Organization)
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
                            EF.Functions.ILike(c.HealthCampStatus.Name.ToLowerInvariant(), "canceled")))
            .Include(c => c.Organization)
            .AsNoTracking()
            .OrderByDescending(c => c.StartDate)
            .ToListAsync(ct);
    }

}
