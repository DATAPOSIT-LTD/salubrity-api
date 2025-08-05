// File: Salubrity.Application/DTOs/Forms/UpdateFormDto.cs
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    public class UpdateFormDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [MaxLength(255)]
        public string? Description { get; set; }

        public List<UpdateFormSectionDto> Sections { get; set; } = [];
    }
}
