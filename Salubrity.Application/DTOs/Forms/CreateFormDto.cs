// File: Salubrity.Application/DTOs/Forms/CreateFormDto.cs
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    public class CreateFormDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        public List<CreateFormSectionDto> Sections { get; set; } = [];
        public List<CreateFormFieldDto> Fields { get; set; } = [];
    }
}
