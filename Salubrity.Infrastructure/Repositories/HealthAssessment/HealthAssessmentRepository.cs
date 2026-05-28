using Microsoft.EntityFrameworkCore;
using Salubrity.Application.DTOs.Forms;
using Salubrity.Application.Interfaces.Repositories.HealthAssessment;
using Salubrity.Domain.Entities.HealthAssesment;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Infrastructure.Repositories.HealthAssessments;

public class HealthAssessmentRepository : IHealthAssessmentRepository
{
    private readonly AppDbContext _db;

    public HealthAssessmentRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HealthAssessment?> GetByIdWithParticipantAsync(Guid assessmentId, CancellationToken ct = default)
    {
        return await _db.HealthAssessments
            .Include(a => a.Participant)
            .FirstOrDefaultAsync(a => a.Id == assessmentId, ct);
    }

    public async Task AddFormResponseAsync(HealthAssessmentFormResponse response, CancellationToken ct = default)
    {
        _db.HealthAssessmentFormResponses.Add(response);


        if (response.Responses?.Any() == true)
        {
            foreach (var r in response.Responses)
            {
                _db.HealthAssessmentDynamicFieldResponses.Add(r);
            }
        }

        await _db.SaveChangesAsync(ct);
    }


    public Task<HealthAssessment> CreateAsync(HealthAssessment entity)
    {
        throw new NotImplementedException();
    }

    public Task<HealthAssessment?> GetByIdAsync(Guid id, bool includeChildren = true)
    {
        throw new NotImplementedException();
    }

    public Task<List<HealthAssessment>> GetByParticipantAsync(Guid participantId)
    {
        throw new NotImplementedException();
    }

    public Task<List<HealthAssessment>> GetByCampAsync(Guid healthCampId)
    {
        throw new NotImplementedException();
    }

    public Task<object?> LoadWithMetricsAsync(Guid assessmentId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<IntakeFormVersion?> GetIntakeFormVersionGraphAsync(Guid intakeFormVersionId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<HealthAssessmentFormResponse?> GetLatestFormResponseAsync(Guid healthAssessmentId, Guid intakeFormVersionId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task<HealthAssessmentFormResponse?> GetLatestFormResponseByFormTypeAsync(Guid formTypeId, Guid intakeFormVersionId, Guid createdByUserId, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<List<PatientAssessmentResponseProjection>> GetPatientResponsesAsync(Guid patientId, Guid campId, CancellationToken ct = default)
    {
        var userId = await _db.Patients
            .Where(p => p.Id == patientId)
            .Select(p => p.UserId)
            .FirstOrDefaultAsync(ct);

        if (userId == Guid.Empty)
            return [];

        var latestResponseId = await _db.HealthAssessmentFormResponses
            .Where(r => r.CreatedBy == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(ct);

        if (latestResponseId == Guid.Empty)
            return [];

        var rawResponses = await _db.HealthAssessmentDynamicFieldResponses
            .Where(r => r.FormResponseId == latestResponseId)
            .Include(r => r.FormResponse.FormType)
            .Include(r => r.Field)
                .ThenInclude(f => f.Section)
            .Include(r => r.Field.Options)
            .ToListAsync(ct);

        var projections = rawResponses
            .Select(r => new PatientAssessmentResponseProjection
            {
                FormName = r.FormResponse.FormType?.Name,
                SectionName = r.Field.Section?.Name,
                SectionOrder = r.Field.Section?.Order ?? 0,
                FieldLabel = r.Field?.Label,
                FieldOrder = r.Field?.Order ?? 0,
                Value = r.Value,
                SelectedOption = r.SelectedOptionId.HasValue
                    ? r.Field.Options.FirstOrDefault(o => o.Id == r.SelectedOptionId)?.Label
                    : null
            })
            .OrderBy(r => r.SectionOrder)
            .ThenBy(r => r.FieldOrder)
            .ToList();

        return projections;
    }





    public async Task<int> SoftDeletePriorSubmissionsAsync(
        Guid userId,
        Guid formTypeId,
        IEnumerable<Guid> sectionIds,
        CancellationToken ct = default)
    {
        var sectionList = sectionIds.Where(s => s != Guid.Empty).Distinct().ToList();
        if (sectionList.Count == 0) return 0;

        var prior = await _db.HealthAssessmentFormResponses
            .Include(r => r.Responses)
            .Where(r => r.CreatedBy == userId
                        && r.FormTypeId == formTypeId
                        && !r.IsDeleted)
            .Where(r => r.Responses.Any(d => d.SectionId.HasValue && sectionList.Contains(d.SectionId.Value)))
            .ToListAsync(ct);

        if (prior.Count == 0) return 0;

        var now = DateTime.UtcNow;
        foreach (var container in prior)
        {
            container.IsDeleted = true;
            container.DeletedAt = now;
            foreach (var dyn in container.Responses)
            {
                dyn.IsDeleted = true;
                dyn.DeletedAt = now;
            }
        }
        await _db.SaveChangesAsync(ct);
        return prior.Count;
    }

    public async Task<Salubrity.Application.DTOs.HealthAssessment.MyHealthAssessmentStatusDto> GetMyStatusAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        var rows = await _db.HealthAssessmentFormResponses
            .AsNoTracking()
            .Where(r => r.CreatedBy == userId && !r.IsDeleted)
            .Select(r => new
            {
                r.FormTypeId,
                r.HealthCampId,
                r.CreatedAt,
                SectionIds = r.Responses
                    .Where(d => !d.IsDeleted && d.SectionId.HasValue)
                    .Select(d => d.SectionId!.Value)
                    .ToList(),
            })
            .ToListAsync(ct);

        var sectionIds = rows
            .SelectMany(r => r.SectionIds)
            .Distinct()
            .ToList();

        // Find earliest camp where user completed all 3 SA form types
        var saTypeIds = new HashSet<Guid>
        {
            Guid.Parse("bcad133b-9b8a-47e5-8551-e86069cdd80d"),
            Guid.Parse("4ab8a5f2-8f8d-4c49-97ec-cdaff0c51843"),
            Guid.Parse("bd5e98e0-7389-4e25-9712-84a7bfe68f63"),
        };

        Guid? completedInCampId = null;
        string? completedInCampName = null;

        var rowsWithCamp = rows.Where(r => r.HealthCampId.HasValue).ToList();
        var campGroups = rowsWithCamp
            .GroupBy(r => r.HealthCampId!.Value)
            .OrderBy(g => rowsWithCamp.Where(r => r.HealthCampId == g.Key).Min(r => r.CreatedAt));

        foreach (var campGroup in campGroups)
        {
            var typesInCamp = campGroup.Select(r => r.FormTypeId).ToHashSet();
            if (saTypeIds.IsSubsetOf(typesInCamp))
            {
                completedInCampId = campGroup.Key;
                completedInCampName = await _db.HealthCamps
                    .Where(c => c.Id == campGroup.Key)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync(ct);
                break;
            }
        }

        return new Salubrity.Application.DTOs.HealthAssessment.MyHealthAssessmentStatusDto
        {
            FormTypeIdsSubmitted = rows.Select(r => r.FormTypeId).Distinct().ToList(),
            SectionIdsSubmitted = sectionIds,
            SectionsSubmittedCount = sectionIds.Count,
            LastSubmittedAt = rows.Count > 0 ? rows.Max(r => r.CreatedAt) : (DateTime?)null,
            CompletedInCampId = completedInCampId,
            CompletedInCampName = completedInCampName,
        };
    }
}
