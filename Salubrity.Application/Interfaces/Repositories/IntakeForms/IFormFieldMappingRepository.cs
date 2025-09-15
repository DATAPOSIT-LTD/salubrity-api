using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Interfaces.Repositories.IntakeForms;

public interface IFormFieldMappingRepository
{
    /// <summary>
    /// Returns a mapping of CSV/Excel column aliases to IntakeFormField IDs for a specific form version.
    /// </summary>
    Task<Dictionary<string, Guid>> GetFieldMappingsAsync(Guid intakeFormVersionId, CancellationToken ct = default);

    /// <summary>
    /// Resolves the IntakeFormVersion entity based on a sheet name from the uploaded workbook.
    /// Returns null if no matching form version exists.
    /// </summary>
    Task<IntakeFormVersion?> GetFormVersionBySheetNameAsync(string sheetName, CancellationToken ct = default);
}
