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

        // True edit semantics: soft-delete any prior live submissions
        // for the same user + formType that touched the same sections,
        // then insert this submission as the new live record.
        var sectionIds = dto.DynamicResponses
            .Where(r => r.SectionId.HasValue && r.SectionId.Value != Guid.Empty)
            .Select(r => r.SectionId!.Value)
            .Distinct()
            .ToList();
        if (sectionIds.Count > 0)
        {
            await _repo.SoftDeletePriorSubmissionsAsync(userId, dto.FormTypeId, sectionIds, ct);
        }

        var formResponse = new HealthAssessmentFormResponse
        {
            Id = Guid.NewGuid(),
            FormTypeId = dto.FormTypeId,
            IntakeFormVersionId = dto.IntakeFormVersionId,
            HealthCampId = dto.HealthCampId,
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
                Sections = [.. formGroup.Select(sec => new AssessmentSectionResponseDto
                {
                    SectionName = sec.Key.SectionName,
                    SectionOrder = sec.Key.SectionOrder,
                    Fields = [.. sec
                        .Select(item => new FieldResponseDto
                        {
                            FieldLabel = item.FieldLabel,
                            FieldOrder = item.FieldOrder,
                            Value = item.Value,
                            SelectedOption = item.SelectedOption
                        })
                        .OrderBy(f => f.FieldOrder)]
                })]
            }).ToList();

        return result;
    }



    public Task<Salubrity.Application.DTOs.HealthAssessment.MyHealthAssessmentStatusDto> GetMyStatusAsync(
        Guid userId,
        CancellationToken ct = default)
    {
        return _repo.GetMyStatusAsync(userId, ct);
    }
}
