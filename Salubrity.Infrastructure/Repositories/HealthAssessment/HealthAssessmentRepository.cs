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



}
