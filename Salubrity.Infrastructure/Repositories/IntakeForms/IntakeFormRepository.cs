// File: Salubrity.Infrastructure/Repositories/Forms/FormRepository.cs

using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.HealthcareServices;
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

    public async Task<IntakeFormVersion?> GetActiveVersionWithFieldsByServiceNameAsync(
        string serviceName,
        CancellationToken ct)
    {
        // 1. Try Service → IntakeForm
        var serviceFormId = await _context.Services
            .Where(s => !s.IsDeleted && s.Name == serviceName && s.IntakeFormId != null)
            .Select(s => s.IntakeFormId.Value)
            .FirstOrDefaultAsync(ct);

        if (serviceFormId != Guid.Empty)
        {
            return await _context.IntakeFormVersions
                .Include(v => v.IntakeForm)
                .Include(v => v.Sections).ThenInclude(s => s.Fields)
                .Where(v => !v.IsDeleted && v.IsActive && v.IntakeFormId == serviceFormId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync(ct);
        }

        // 2. Try ServiceCategory → IntakeForm
        var categoryFormId = await _context.ServiceCategories
            .Where(c => !c.IsDeleted && c.Name == serviceName && c.IntakeFormId != null)
            .Select(c => c.IntakeFormId.Value)
            .FirstOrDefaultAsync(ct);

        if (categoryFormId != Guid.Empty)
        {
            return await _context.IntakeFormVersions
                .Include(v => v.IntakeForm)
                .Include(v => v.Sections).ThenInclude(s => s.Fields)
                .Where(v => !v.IsDeleted && v.IsActive && v.IntakeFormId == categoryFormId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync(ct);
        }

        // 3. Try ServiceSubcategory → IntakeForm
        var subcategoryFormId = await _context.ServiceSubcategories
            .Where(sc => !sc.IsDeleted && sc.Name == serviceName && sc.IntakeFormId != null)
            .Select(sc => sc.IntakeFormId.Value)
            .FirstOrDefaultAsync(ct);

        if (subcategoryFormId != Guid.Empty)
        {
            return await _context.IntakeFormVersions
                .Include(v => v.IntakeForm)
                .Include(v => v.Sections).ThenInclude(s => s.Fields)
                .Where(v => !v.IsDeleted && v.IsActive && v.IntakeFormId == subcategoryFormId)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync(ct);
        }

        // Nothing matched
        return null;
    }


    public async Task<IntakeFormVersion?> ResolveFormVersionByAssignmentAsync(
        Guid assignmentId,
        PackageItemType assignmentType,
        CancellationToken ct = default)
    {
        IntakeForm? form = null;

        switch (assignmentType)
        {
            case PackageItemType.Service:
                form = await _context.Services
                    .Where(s => s.Id == assignmentId && s.IntakeFormId != null)
                    .Select(s => s.IntakeForm!)
                    .FirstOrDefaultAsync(ct);
                break;

            case PackageItemType.ServiceCategory:
                form = await _context.ServiceCategories
                    .Where(c => c.Id == assignmentId && c.IntakeFormId != null)
                    .Select(c => c.IntakeForm!)
                    .FirstOrDefaultAsync(ct);
                break;

            case PackageItemType.ServiceSubcategory:
                form = await _context.ServiceSubcategories
                    .Where(sc => sc.Id == assignmentId && sc.IntakeFormId != null)
                    .Select(sc => sc.IntakeForm!)
                    .FirstOrDefaultAsync(ct);
                break;
        }

        if (form == null)
            return null;

        // Get active version with sections + fields
        return await _context.IntakeFormVersions
            .Include(v => v.Sections)
                .ThenInclude(s => s.Fields)
            .Where(v => v.IntakeFormId == form.Id && v.IsActive && !v.IsDeleted)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct);
    }

}
