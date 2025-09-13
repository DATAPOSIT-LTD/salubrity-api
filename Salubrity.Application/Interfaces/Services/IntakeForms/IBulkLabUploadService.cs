using Salubrity.Application.DTOs.IntakeForms;

namespace Salubrity.Application.Interfaces.Services.IntakeForms;

public interface IBulkLabUploadService
{
    /// <summary>
    /// Upload an Excel file containing lab results.
    /// The system determines the correct IntakeFormVersion and maps to patients.
    /// </summary>
    Task<BulkUploadResultDto> UploadExcelAsync(CreateBulkLabUploadDto dto, CancellationToken ct = default);

    /// <summary>
    /// Generate a single Excel workbook containing all lab forms with headers and patient info.
    /// </summary>
    Task<Stream> GenerateAllLabTemplatesExcelAsync(CancellationToken ct = default);
}
