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



}
