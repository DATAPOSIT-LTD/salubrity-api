
namespace Salubrity.Application.Interfaces.Services.HealthAssessment;

public interface IHealthAssessmentService
{
    Task<HealthAssessmentDto> CreateAsync(CreateHealthAssessmentDto dto);
    Task<HealthAssessmentDto> GetByIdAsync(Guid id);
    Task<List<HealthAssessmentDto>> GetByParticipantAsync(Guid participantId);
    Task<List<HealthAssessmentDto>> GetByCampAsync(Guid healthCampId);
}
