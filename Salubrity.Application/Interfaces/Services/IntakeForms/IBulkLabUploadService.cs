using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
    /// Generate a single Excel workbook containing all lab forms with headers and patient info,
    /// scoped to the specific camp the user is assigned to.
    /// </summary>
    Task<Stream> GenerateLabTemplateForCampAsync(Guid userId, Guid campId, CancellationToken ct = default);
}
