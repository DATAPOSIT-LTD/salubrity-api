using System;
using System.Threading;
using System.Threading.Tasks;
using Salubrity.Application.DTOs.HealthAssessments;

namespace Salubrity.Application.Interfaces.Services.HealthAssessments;

public interface IHealthAssessmentFormService
{
    Task<Guid> SubmitFormSectionAsync(SubmitHealthAssessmentFormDto dto, Guid userId, CancellationToken ct = default);
    Task<List<HealthAssessmentResponseDto>> GetPatientAssessmentResponsesAsync(Guid patientId, Guid campId, CancellationToken ct = default);
}
