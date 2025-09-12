using Salubrity.Application.DTOs.IntakeForms;

namespace Salubrity.Application.Interfaces.Services.IntakeForms;

public interface IBulkLabUploadService
{
    /// <summary>
    /// Upload a CSV file containing lab results. 
    /// The server will determine the correct IntakeFormVersion based on the file/sheet name.
    /// </summary>
    Task<BulkUploadResultDto> UploadCsvAsync(CreateBulkLabUploadDto dto, CancellationToken ct = default);

    /// <summary>
    /// Generate a CSV template for a lab results form.
    /// The form version will be determined internally from the given file/sheet name identifier.
    /// </summary>
    /// <param name="fileIdentifier">File name or sheet name used to determine form version</param>
    Task<Stream> GenerateCsvTemplateAsync(string fileIdentifier, CancellationToken ct = default);
}
