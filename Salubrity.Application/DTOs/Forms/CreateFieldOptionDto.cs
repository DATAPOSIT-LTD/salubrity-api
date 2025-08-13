// File: Salubrity.Application/DTOs/Forms/CreateFieldOptionDto.cs
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Forms
{
    public class CreateFieldOptionDto
    {
        [Required, StringLength(200)]
        public string Value { get; set; } = null!;

        [Required, StringLength(200)]
        public string DisplayText { get; set; } = null!;

        public int Order { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }
}
