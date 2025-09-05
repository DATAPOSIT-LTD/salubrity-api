// HealthAssessmentResponseDto.cs
namespace Salubrity.Application.DTOs.HealthAssessments;

public class HealthAssessmentResponseDto
{
    public string FormName { get; set; } = default!;
    public List<FormSectionResponseDto> Sections { get; set; } = new();
}

public class FormSectionResponseDto
{
    public string SectionName { get; set; } = default!;
    public int SectionOrder { get; set; }
    public List<FieldResponseDto> Fields { get; set; } = new();
}

public class FieldResponseDto
{
    public string FieldLabel { get; set; } = default!;
    public int FieldOrder { get; set; }
    public string? Value { get; set; }
    public string? SelectedOption { get; set; }
}
