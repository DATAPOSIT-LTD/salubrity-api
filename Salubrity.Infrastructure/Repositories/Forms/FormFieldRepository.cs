using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Forms;
using Salubrity.Domain.Entities.FormFields;
using Salubrity.Infrastructure.Persistence;

public class FormFieldRepository : IFormFieldRepository
{
    private readonly AppDbContext _context;

    public FormFieldRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<FormField?> GetByIdAsync(Guid fieldId)
    {
        return await _context.FormFields.FindAsync(fieldId);
    }

    public async Task<FormField?> GetFormFieldWithOptionsAsync(Guid fieldId)
    {
        return await _context.FormFields
            .Include(f => f.Options)
            .FirstOrDefaultAsync(f => f.Id == fieldId);
    }

    public async Task<List<FormField>> GetByFormIdAsync(Guid formId)
    {
        return await _context.FormFields
            .Where(f => f.FormId == formId)
            .ToListAsync();
    }

    public async Task<List<FormField>> GetBySectionIdAsync(Guid sectionId)
    {
        return await _context.FormFields
            .Where(f => f.SectionId == sectionId)
            .ToListAsync();
    }

    public async Task<FormField> CreateAsync(FormField field)
    {
        _context.FormFields.Add(field);
        await _context.SaveChangesAsync();
        return field;
    }

    public async Task<FormField> UpdateAsync(FormField field)
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
