using Salubrity.Application.DTOs.HealthAssessments;
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


    public async Task<List<HealthAssessmentResponseDto>> GetPatientAssessmentResponsesAsync(Guid patientId, Guid campId, CancellationToken ct = default)
    {
        var assessments = await _repo.GetPatientResponsesAsync(patientId, campId, ct);

        var result = assessments
            .GroupBy(r => new { r.FormName, r.SectionName, r.SectionOrder })
            .OrderBy(g => g.Key.SectionOrder)
            .GroupBy(g => g.Key.FormName)
            .Select(formGroup => new HealthAssessmentResponseDto
            {
                FormName = formGroup.Key,
                Sections = formGroup.Select(sec => new AssessmentSectionResponseDto
                {
                    SectionName = sec.Key.SectionName,
                    SectionOrder = sec.Key.SectionOrder,
                    Fields = sec
                        .Select(item => new FieldResponseDto
                        {
                            FieldLabel = item.FieldLabel,
                            FieldOrder = item.FieldOrder,
                            Value = item.Value,
                            SelectedOption = item.SelectedOption
                        })
                        .OrderBy(f => f.FieldOrder)
                        .ToList()
                }).ToList()
            }).ToList();

        return result;
    }

}
