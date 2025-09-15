using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.IntakeForms;

public class FormFieldMappingRepository : IFormFieldMappingRepository
{
    private readonly AppDbContext _db;

    public FormFieldMappingRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Dictionary<string, Guid>> GetFieldMappingsAsync(Guid intakeFormVersionId, CancellationToken ct = default)
    {
        return await _db.FormFieldMappings
            .Where(m => m.IntakeFormVersionId == intakeFormVersionId)
            .ToDictionaryAsync(m => m.Alias.ToLower(), m => m.IntakeFormFieldId, ct);
    }

    public async Task<IntakeFormVersion?> GetFormVersionBySheetNameAsync(string sheetName, CancellationToken ct = default)
    {
        return await _db.IntakeFormVersions
            .FirstOrDefaultAsync(fv => fv.IntakeForm.Name == sheetName, ct);
    }
}
