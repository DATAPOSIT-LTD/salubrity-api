// File: Salubrity.Application/DTOs/Forms/FormSectionResponseDto.cs
namespace Salubrity.Application.DTOs.Forms
{
    public class FormSectionResponseDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string? Description { get; set; }

        public int Order { get; set; }

        public List<FormFieldResponseDto> Fields { get; set; } = [];
    }
}
