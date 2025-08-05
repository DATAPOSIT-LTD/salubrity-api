// File: Salubrity.Application/DTOs/Forms/FormFieldResponseDto.cs
namespace Salubrity.Application.DTOs.Forms
{
    public class FormFieldResponseDto
    {
        public Guid Id { get; set; }

        public Guid SectionId { get; set; }

        public string Label { get; set; } = null!;

        public string FieldType { get; set; } = null!;

        public bool IsRequired { get; set; }

        public int Order { get; set; }

        public List<FieldOptionResponseDto> Options { get; set; } = [];
    }
}
