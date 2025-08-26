using System;
using System.Threading;
using System.Threading.Tasks;

namespace Salubrity.Application.Interfaces.Services.HealthAssessments;

public interface IHealthAssessmentFormService
{
    Task<Guid> SubmitFormSectionAsync(SubmitHealthAssessmentFormDto dto, Guid userId, CancellationToken ct = default);
}
