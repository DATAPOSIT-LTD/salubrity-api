using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Application.Interfaces;
using Salubrity.Shared.Exceptions;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;

namespace Salubrity.Infrastructure.Persistence.Repositories.IntakeForms;

public sealed class IntakeFormResponseRepository : IIntakeFormResponseRepository
{
    private readonly AppDbContext _db;

    public IntakeFormResponseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IntakeFormResponse> AddAsync(IntakeFormResponse response, CancellationToken ct = default)
    {
        _db.IntakeFormResponses.Add(response);
        await _db.SaveChangesAsync(ct);
        return response;
    }

    public Task<IntakeFormResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return _db.IntakeFormResponses
            .AsNoTracking()
            .Include(r => r.FieldResponses)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public Task<bool> IntakeFormVersionExistsAsync(Guid versionId, CancellationToken ct = default)
    {
        return _db.IntakeFormVersions
            .AsNoTracking()
            .AnyAsync(v => v.Id == versionId, ct);
    }

    // public async Task<HashSet<Guid>> GetFieldIdsForVersionAsync(Guid versionId, CancellationToken ct = default)
    // {
    //     return (await _db.IntakeFormFields
    //         .AsNoTracking()
    //         .Where(f => f.Form.Versions.Contains == versionId)
    //         .Select(f => f.Id)
    //         .ToListAsync(ct))
    //         .ToHashSet();
    // }

    public async Task<HashSet<Guid>> GetFieldIdsForVersionAsync(Guid versionId, CancellationToken ct = default)
    {
        return await _db.IntakeFormFields
            .AsNoTracking()
            .Where(f => f.Section.IntakeFormVersionId == versionId)
            .Select(f => f.Id)
            .ToHashSetAsync(ct);
    }
    public async Task<Guid> GetStatusIdByNameAsync(string name, CancellationToken ct = default)
    {
        var id = await _db.IntakeFormResponseStatuses
            .Where(s => s.Name == name && !s.IsDeleted)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(ct);

        if (id == Guid.Empty)
            throw new NotFoundException($"Status with name '{name}' not found.");

        return id;
    }


    public async Task<List<IntakeFormResponseDetailDto>> GetResponsesByPatientAndCampIdAsync(Guid patientId, Guid healthCampId, CancellationToken ct = default)
    {
        var query =
            from r in _db.IntakeFormResponses
                .Include(r => r.FieldResponses).ThenInclude(fr => fr.Field).ThenInclude(f => f.Section)
                .Include(r => r.Status)
                .Include(r => r.Version).ThenInclude(v => v.IntakeForm)
                .Include(r => r.Service)
            join hca in _db.HealthCampServiceAssignments
                on r.ServiceId equals hca.ServiceId
            where r.PatientId == patientId
                  && hca.HealthCampId == healthCampId
            orderby r.CreatedAt descending
            select new IntakeFormResponseDetailDto
            {
                Id = r.Id,
                IntakeFormVersionId = r.IntakeFormVersionId,
                SubmittedByUserId = r.SubmittedByUserId,
                PatientId = r.PatientId,
                ServiceId = r.ServiceId,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt,
                Status = new ResponseStatusDto
                {
                    Id = r.ResponseStatusId,
                    Name = r.Status.Name
                },
                Version = new MiniIntakeFormVersionDto
                {
                    Id = r.Version.Id,
                    IntakeFormId = r.Version.IntakeFormId,
                    VersionNumber = r.Version.VersionNumber,
                    IntakeFormName = r.Version.IntakeForm.Name,
                    IntakeFormDescription = r.Version.IntakeForm.Description
                },
                Service = r.Service == null ? null : new MiniServiceDto
                {
                    Id = r.Service.Id,
                    Name = r.Service.Name,
                    Description = r.Service.Description,
                    ImageUrl = r.Service.ImageUrl
                },
                FieldResponses = r.FieldResponses
                    .OrderBy(fr => fr.Field.Order)
                    .Select(fr => new IntakeFormFieldResponseDetailDto
                    {
                        Id = fr.Id,
                        FieldId = fr.FieldId,
                        Value = fr.Value,
                        Field = new FieldMetaDto
                        {
                            FieldId = fr.Field.Id,
                            Label = fr.Field.Label,
                            FieldType = fr.Field.FieldType,
                            SectionId = fr.Field.SectionId,
                            SectionName = fr.Field.Section != null ? fr.Field.Section.Name : null,
                            Order = fr.Field.Order
                        }
                    }).ToList()
            };

        return await query.ToListAsync(ct);
    }


}

