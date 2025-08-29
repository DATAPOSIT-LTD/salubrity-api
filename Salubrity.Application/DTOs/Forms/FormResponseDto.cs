// File: Salubrity.Application/DTOs/Forms/FormResponseDto.cs
namespace Salubrity.Application.DTOs.Forms
{
    public class FormResponseDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }
        public Guid? IntakeFormVersionId { get; set; }

        // One of these two will be populated, never both
        public List<FormSectionResponseDto> Sections { get; set; } = [];
        public List<FormFieldResponseDto> Fields { get; set; } = [];

    }


    // Returned to the frontend
    public sealed class HealthAssessmentFormViewDto
    {
        public Guid IntakeFormVersionId { get; init; }
        public string FormName { get; init; } = default!;
        public List<FormSectionViewDto> Sections { get; init; } = new();
    }

    public sealed class FormSectionViewDto
    {
        public Guid SectionId { get; init; }
        public string SectionName { get; init; } = default!;
        public int Order { get; init; }
        public List<FormFieldViewDto> Fields { get; init; } = new();
    }

    public sealed class FormFieldViewDto
    {
        public Guid FieldId { get; init; }
        public string Label { get; init; } = default!;
        public string FieldType { get; init; } = default!; // e.g. Text, TextArea, Number, Dropdown, Boolean
        public string? Value { get; init; }                // For text/number/textarea values
        public Guid? SelectedOptionId { get; init; }       // For option-based fields
        public string? SelectedOptionText { get; init; }   // Resolved display text
        public bool? AsBoolean { get; init; }              // Convenience for Yes/No style toggles
        public int Order { get; init; }
    }

}
