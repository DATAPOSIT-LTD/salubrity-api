using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.Interfaces;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Shared.Exceptions;

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

    public async Task<List<IntakeFormResponseDetailDto>> GetResponsesByPatientAndCampIdAsync(
     Guid? patientId, Guid healthCampId, CancellationToken ct = default)
    {
        var query =
            from r in _db.IntakeFormResponses
                .Include(r => r.FieldResponses)
                    .ThenInclude(fr => fr.Field)
                        .ThenInclude(f => f.Section)
                .Include(r => r.Status)
                .Include(r => r.Version)
                    .ThenInclude(v => v.IntakeForm)
                .Include(r => r.ResolvedService)
            join a in _db.HealthCampServiceAssignments
                on r.SubmittedServiceId equals a.AssignmentId
            where r.PatientId == patientId
                  && a.HealthCampId == healthCampId
            orderby r.CreatedAt descending
            select new IntakeFormResponseDetailDto
            {
                Id = r.Id,
                IntakeFormVersionId = r.IntakeFormVersionId,
                SubmittedByUserId = r.SubmittedByUserId,
                PatientId = r.PatientId,
                ServiceId = r.ResolvedServiceId,
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
                Service = r.ResolvedService == null ? null : new MiniServiceDto
                {
                    Id = r.ResolvedService.Id,
                    Name = r.ResolvedService.Name,
                    Description = r.ResolvedService.Description,
                    ImageUrl = r.ResolvedService.ImageUrl
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

    public async Task<List<IntakeFormResponse>> GetResponsesByCampIdWithDetailsAsync(Guid campId, CancellationToken ct = default)
    {
        return await _db.IntakeFormResponses
            .Include(r => r.Patient)
            .Include(r => r.FieldResponses)
                .ThenInclude(fr => fr.Field)
            .Where(r => r.PatientId != null &&
                       _db.HealthCampParticipants.Any(p => p.Id == r.PatientId && p.HealthCampId == campId) &&
                       !r.IsDeleted)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<IntakeFormResponse>> GetByCampAndAssignmentAsync(
    Guid campId,
    Guid assignmentId,
    PackageItemType assignmentType,
    CancellationToken ct = default)
    {
        var query = _db.IntakeFormResponses
            .AsNoTracking()
            .Include(r => r.Patient)
                .ThenInclude(p => p.User)
                    .ThenInclude(u => u.Gender)
            .Include(r => r.Status)
            .Include(r => r.FieldResponses)
                .ThenInclude(fr => fr.Field)
            .Where(r =>
                !r.IsDeleted &&
                r.SubmittedServiceId == assignmentId &&
                r.SubmittedServiceType == assignmentType &&
                _db.HealthCampParticipants.Any(p =>
                    p.HealthCampId == campId &&
                    p.PatientId == r.PatientId &&
                    !p.IsDeleted));

        return await query
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);
    }

    //Download Findings Implementation

    //public async Task<List<IntakeFormResponse>> GetResponsesByCampIdWithDetailAsync(Guid campId, CancellationToken ct = default)
    //{
    //    return await _db.IntakeFormResponses
    //        .Include(r => r.Patient)
    //            .ThenInclude(p => p.User)
    //        .Include(r => r.FieldResponses)
    //            .ThenInclude(fr => fr.Field)
    //        .Where(r => r.PatientId != null &&
    //                   _db.HealthCampParticipants.Any(p => p.Id == r.PatientId && p.HealthCampId == campId) &&
    //                   !r.IsDeleted)
    //        .OrderBy(r => r.Patient.User.FirstName)
    //        .ThenBy(r => r.Patient.User.LastName)
    //        .ToListAsync(ct);
    //}

    public async Task<List<IntakeFormResponse>> GetResponsesByCampIdWithDetailAsync(Guid campId, CancellationToken ct = default)
    {
        return await (from r in _db.IntakeFormResponses
                          .Include(r => r.Patient)
                              .ThenInclude(p => p.User)
                                .ThenInclude(u => u.Gender)
                          .Include(r => r.FieldResponses)
                              .ThenInclude(fr => fr.Field)
                      join a in _db.HealthCampServiceAssignments
                          on r.SubmittedServiceId equals a.AssignmentId
                      where r.PatientId != null
                            && a.HealthCampId == campId
                            && !r.IsDeleted
                      orderby r.Patient.User.FirstName, r.Patient.User.LastName
                      select r)
            .ToListAsync(ct);
    }
}