// File: Salubrity.Application/DTOs/Forms/CreateFormFieldDto.cs
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    public class CreateFormFieldDto
    {
        public Guid SectionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Label { get; set; } = null!;

        [Required]
        public string FieldType { get; set; } = null!;

        public bool IsRequired { get; set; }

        public int Order { get; set; }

        public List<CreateFieldOptionDto> Options { get; set; } = [];
    }
}
