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
}
