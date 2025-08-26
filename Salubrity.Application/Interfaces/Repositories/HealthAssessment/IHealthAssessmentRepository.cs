using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Interfaces.Repositories.HealthAssessment;

public interface IHealthAssessmentRepository
{
    Task<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment> CreateAsync(Salubrity.Domain.Entities.HealthAssesment.HealthAssessment entity);
    Task<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment?> GetByIdAsync(Guid id, bool includeChildren = true);
    Task<List<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment>> GetByParticipantAsync(Guid participantId);
    Task<List<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment>> GetByCampAsync(Guid healthCampId);
    Task<object?> LoadWithMetricsAsync(Guid assessmentId, CancellationToken ct);

    Task<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment?> GetByIdWithParticipantAsync(Guid assessmentId, CancellationToken ct = default);
    Task AddFormResponseAsync(HealthAssessmentFormResponse response, CancellationToken ct = default);

    // Existing

    // New: load the full form blueprint (version + sections + fields + options)
    Task<IntakeFormVersion?> GetIntakeFormVersionGraphAsync(Guid intakeFormVersionId, CancellationToken ct);

    // New: get the most recent response for this Assessment + Version (or by FormType, if that’s your key)
    Task<HealthAssessmentFormResponse?> GetLatestFormResponseAsync(
        Guid healthAssessmentId,
        Guid intakeFormVersionId,
        CancellationToken ct);

    // Optional: if your write path currently doesn’t store HealthAssessmentId,
    // add a variant keyed by CreatedBy or FormTypeId:
    Task<HealthAssessmentFormResponse?> GetLatestFormResponseByFormTypeAsync(
        Guid formTypeId,
        Guid intakeFormVersionId,
        Guid createdByUserId,
        CancellationToken ct);



}
