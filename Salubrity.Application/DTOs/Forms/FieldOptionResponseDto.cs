// File: Salubrity.Application/DTOs/Forms/FieldOptionResponseDto.cs
namespace Salubrity.Application.DTOs.Forms
{
    public class FieldOptionResponseDto
    {
        public Guid Id { get; set; }

        public string Value { get; set; } = null!;

        public string DisplayText { get; set; } = null!;
    }
}
