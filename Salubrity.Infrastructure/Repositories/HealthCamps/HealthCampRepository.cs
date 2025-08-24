using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Forms;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthCamps;
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
            .Where(a => a.SubcontractorId == subcontractorId)
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
                (c.CloseDate == null || c.CloseDate > nowUtc) &&
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
            .Where(c => c.IsLaunched
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
            .Include(x => x.Service)
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
                (x.HealthCamp.HealthCampStatus != null && x.HealthCamp.HealthCampStatus.Name == HealthCampStatusNames.Suspended)),

            _ => baseQuery
        };

        // Materialize everything first
        var assignments = await baseQuery.ToListAsync(ct);

        // Group and project in-memory
        var result = assignments
            .GroupBy(x => x.HealthCamp)
            .Select(g => new HealthCampWithRolesDto
            {
                CampId = g.Key.Id,
                ClientName = g.Key.Organization?.BusinessName ?? "—",
                Venue = g.Key.Location ?? "—",
                StartDate = g.Key.StartDate,
                EndDate = g.Key.EndDate,
                Status = g.Key.HealthCampStatus?.Name ?? "Unknown",
                Roles = g
                    .Select(r => new RoleAssignmentDto
                    {
                        AssignedBooth = r.Service?.Name ?? "—",
                        AssignedRole = r.Role?.Name ?? "—"
                    })
                    .Distinct()
                    .ToList()
            })
            .ToList();

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
            .Include(p => p.HealthAssessments) // ✅ include to avoid Any() failure
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
        // 1) Participant + Camp + Org + User
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

        if (p == null) return null;

        var served = p.Participant.ParticipatedAt != null
                     || await _context.HealthAssessments
                            .AnyAsync(a => a.ParticipantId == participantId, ct);

        // 2) Assignments for THIS camp (+ optional subcontractor filter)
        IQueryable<HealthCampServiceAssignment> assignQ = _context.HealthCampServiceAssignments
            .Where(a => a.HealthCampId == campId)
            .Include(a => a.Service)
                .ThenInclude(s => s.IntakeForm)
                    .ThenInclude(f => f.Sections)
                        .ThenInclude(sec => sec.Fields)
                            .ThenInclude(ff => ff.Options)
            .Include(a => a.Role); // 

        if (subcontractorId.HasValue)
            assignQ = assignQ.Where(a => a.SubcontractorId == subcontractorId.Value);

        var assignments = await assignQ
            .AsNoTracking()
            .ToListAsync(ct);

        // 3) Shape DTO
        var dto = new CampPatientDetailWithFormsDto
        {
            ParticipantId = p.Participant.Id,
            UserId = p.User.Id,
            PatientCode = p.Participant.PatientId.ToString() ?? "N/A",
            FullName = p.User.FullName ?? "Patient",
            Email = p.User.Email,
            Phone = p.User.Phone,
            CampId = p.Camp.Id,
            ClientName = p.OrgName,
            Venue = p.Venue ?? "",
            StartDate = p.Camp.StartDate,
            EndDate = p.Camp.EndDate,
            Status = p.Status,
            Served = served
        };

        // 4) Project each service (distinct by ServiceId to avoid duplicates if any)
        dto.Assignments = assignments
            .GroupBy(a => a.ServiceId)
            .Select(g =>
            {
                var any = g.First();
                return new AssignedServiceWithFormDto
                {
                    ServiceId = any.ServiceId,
                    ServiceName = any.Service.Name,
                    ProfessionId = any.ProfessionId,
                    AssignedRole = any.Role?.Name, // ✅ Directly from SubcontractorRole
                    Form = MapForm(any.Service.IntakeForm) // may be null
                };
            })
            .OrderBy(x => x.ServiceName)
            .ToList();

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
}
