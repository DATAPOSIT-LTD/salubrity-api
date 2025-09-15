// File: Salubrity.Infrastructure/Repositories/Forms/FormRepository.cs

using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Infrastructure.Persistence;

public class IntakeFormRepository : IIntakeFormRepository
{
    private readonly AppDbContext _context;

    public IntakeFormRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IntakeForm?> GetByIdAsync(Guid id)
    {
        return await _context.IntakeForms
            .Include(f => f.Sections)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<List<IntakeForm>> GetAllAsync()
    {
        return await _context.IntakeForms
            .Include(f => f.Sections)
            .ToListAsync();
    }

    public async Task<IntakeForm> CreateAsync(IntakeForm entity)
    {
        _context.IntakeForms.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<IntakeForm> UpdateAsync(IntakeForm entity)
    {
        _context.IntakeForms.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(Guid id)
    {
        var form = await _context.IntakeForms.FindAsync(id);
        if (form is not null)
        {
            _context.IntakeForms.Remove(form);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> FormExistsAsync(Guid formId)
    {
        return await _context.IntakeForms.AnyAsync(f => f.Id == formId);
    }

    public async Task<int> GetFormCountAsync()
    {
        return await _context.IntakeForms.CountAsync();
    }

    //  Implements: GetWithSectionsAsync — full tree: Sections -> Fields -> Options
    public async Task<IntakeForm?> GetWithSectionsAsync(Guid formId)
    {
        return await _context.IntakeForms
            .Include(f => f.Sections)
                .ThenInclude(s => s.Fields)
                    .ThenInclude(f => f.Options.OrderBy(o => o.Order))
            .FirstOrDefaultAsync(f => f.Id == formId);
    }

    //  Implements: GetWithFieldsAsync — direct fields on form (no sections)
    public async Task<IntakeForm?> GetWithFieldsAsync(Guid formId)
    {
        return await _context.IntakeForms
            .Include(f => f.Fields)
                .ThenInclude(ff => ff.Options)
            .FirstOrDefaultAsync(f => f.Id == formId);
    }
    public async Task<bool> IsFormAssignedAnywhereAsync(Guid formId)
    {
        var isUsedByService = await _context.Services.AnyAsync(s => s.IntakeFormId == formId);
        var isUsedByCategory = await _context.ServiceCategories.AnyAsync(c => c.IntakeFormId == formId);
        var isUsedBySubcategory = await _context.ServiceSubcategories.AnyAsync(sc => sc.IntakeFormId == formId);

        return isUsedByService || isUsedByCategory || isUsedBySubcategory;
    }
    /// <summary>
    /// Fetch all forms that are marked as lab forms (IsLabForm = true), including sections and fields.
    /// </summary>
    public async Task<List<IntakeForm>> GetLabFormsByIdsAsync(HashSet<Guid> ids, CancellationToken ct)
    {
        return await _context.IntakeForms
            .Where(f => f.IsLabForm && ids.Contains(f.Id))
            .Include(f => f.Sections.OrderBy(s => s.Order))
                .ThenInclude(s => s.Fields.OrderBy(fld => fld.Order))
                    .ThenInclude(fld => fld.Options.OrderBy(opt => opt.Order))
            .ToListAsync(ct);
    }

    public async Task<IntakeFormVersion?> GetActiveVersionWithFieldsByFormNameAsync(string formName, CancellationToken ct)
    {
        return await _context.IntakeFormVersions
            .Include(v => v.IntakeForm)
            .Include(v => v.Sections)
                .ThenInclude(s => s.Fields)
            .Where(v => !v.IsDeleted
                && v.IsActive
                && v.IntakeForm.Name == formName)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct);
    }


}
