using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Infrastructure.Persistence;

public class FormFieldRepository : IFormFieldRepository
{
    private readonly AppDbContext _context;

    public FormFieldRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IntakeFormField?> GetByIdAsync(Guid fieldId)
    {
        return await _context.FormFields.FindAsync(fieldId);
    }

    public async Task<IntakeFormField?> GetFormFieldWithOptionsAsync(Guid fieldId)
    {
        return await _context.FormFields
            .Include(f => f.Options)
            .FirstOrDefaultAsync(f => f.Id == fieldId);
    }

    public async Task<List<IntakeFormField>> GetByFormIdAsync(Guid formId)
    {
        return await _context.FormFields
            .Where(f => f.FormId == formId)
            .ToListAsync();
    }

    public async Task<List<IntakeFormField>> GetBySectionIdAsync(Guid sectionId)
    {
        return await _context.FormFields
            .Where(f => f.SectionId == sectionId)
            .ToListAsync();
    }

    public async Task<IntakeFormField> CreateAsync(IntakeFormField field)
    {
        _context.FormFields.Add(field);
        await _context.SaveChangesAsync();
        return field;
    }

    public async Task<IntakeFormField> UpdateAsync(IntakeFormField field)
    {
        _context.FormFields.Update(field);
        await _context.SaveChangesAsync();
        return field;
    }

    public async Task DeleteAsync(Guid fieldId)
    {
        var field = await _context.FormFields.FindAsync(fieldId);
        if (field != null)
        {
            _context.FormFields.Remove(field);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> FieldExistsInFormAsync(Guid formId, Guid fieldId)
    {
        return await _context.FormFields
            .AnyAsync(f => f.FormId == formId && f.Id == fieldId);
    }
}
