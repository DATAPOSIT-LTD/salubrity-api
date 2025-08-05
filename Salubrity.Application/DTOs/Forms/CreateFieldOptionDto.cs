// File: Salubrity.Application/DTOs/Forms/CreateFieldOptionDto.cs
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    public class CreateFieldOptionDto
    {
        [Required]
        public string Value { get; set; } = null!;

        [Required]
        public string DisplayText { get; set; } = null!;
    }
}
