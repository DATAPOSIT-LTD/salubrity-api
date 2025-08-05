// File: Salubrity.Application/DTOs/Forms/UpdateFormSectionDto.cs
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    public class UpdateFormSectionDto
    {
        public Guid? Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        public int Order { get; set; }

        public List<UpdateFormFieldDto> Fields { get; set; } = [];
    }
}
