// File: Application/DTOs/IntakeForms/IntakeFormResponseExportDto.cs
namespace Salubrity.Application.DTOs.IntakeForms
{
    public class IntakeFormResponseExportDto
    {
        public byte[] Content { get; set; } = default!;

        public string ContentType { get; set; } = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public string FileName { get; set; } = default!;
    }
}
