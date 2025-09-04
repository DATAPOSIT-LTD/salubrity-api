using Salubrity.Application.Interfaces.Repositories.HealthAssessment;
using Salubrity.Application.Interfaces.Services.HealthAssessments;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.HealthAssessments;

public class HealthAssessmentFormService : IHealthAssessmentFormService
{
    private readonly IHealthAssessmentRepository _repo;

    public HealthAssessmentFormService(IHealthAssessmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> SubmitFormSectionAsync(SubmitHealthAssessmentFormDto dto, Guid userId, CancellationToken ct = default)
    {

        var formResponse = new HealthAssessmentFormResponse
        {
            Id = Guid.NewGuid(),
            FormTypeId = dto.FormTypeId,
            IntakeFormVersionId = dto.IntakeFormVersionId,
            CreatedBy = userId,
            Responses = [.. dto.DynamicResponses.Select(r => new HealthAssessmentDynamicFieldResponse
            {
                Id = Guid.NewGuid(),
                FieldId = r.FieldId,
                Value = r.Value,
                SelectedOptionId = r.SelectedOptionId,
                SectionId = r.SectionId
            })]
        };

        await _repo.AddFormResponseAsync(formResponse, ct);

        return formResponse.Id;
    }
}
