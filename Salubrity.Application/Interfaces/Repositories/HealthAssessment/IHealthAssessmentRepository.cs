using Salubrity.Domain.Entities.HealthAssesment;

namespace Salubrity.Application.Interfaces.Repositories.HealthAssessment;

public interface IHealthAssessmentRepository
{
    Task<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment> CreateAsync(Salubrity.Domain.Entities.HealthAssesment.HealthAssessment entity);
    Task<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment?> GetByIdAsync(Guid id, bool includeChildren = true);
    Task<List<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment>> GetByParticipantAsync(Guid participantId);
    Task<List<Salubrity.Domain.Entities.HealthAssesment.HealthAssessment>> GetByCampAsync(Guid healthCampId);
}
