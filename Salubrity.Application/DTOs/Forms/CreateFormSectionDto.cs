// File: Salubrity.Application/DTOs/Forms/CreateFormSectionDto.cs
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    public class CreateFormSectionDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        public int Order { get; set; }

        public List<CreateFormFieldDto> Fields { get; set; } = [];
    }
}
