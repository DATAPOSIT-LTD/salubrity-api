// File: Salubrity.Application/DTOs/Forms/UpdateFieldOptionDto.cs
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    public class UpdateFieldOptionDto
    {
        public Guid? Id { get; set; }

        [Required]
        public string Value { get; set; } = null!;

        [Required]
        public string DisplayText { get; set; } = null!;
    }
}
