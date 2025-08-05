// File: Salubrity.Application/DTOs/Forms/UpdateFormFieldDto.cs
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    public class UpdateFormFieldDto
    {
        public Guid? Id { get; set; }

        public Guid SectionId { get; set; }

        [Required]
        [StringLength(100)]
        public string Label { get; set; } = null!;

        [Required]
        public string FieldType { get; set; } = null!;

        public bool IsRequired { get; set; }

        public int Order { get; set; }

        public List<UpdateFieldOptionDto> Options { get; set; } = [];
    }
}
