using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Forms;
using Salubrity.Domain.Entities.FormFields;
using Salubrity.Domain.Entities.Forms;
using Salubrity.Infrastructure.Persistence;

public class FormRepository : IFormRepository
{
    private readonly AppDbContext _context;

    public FormRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Form> GetByIdAsync(Guid id)
    {
        return await _context.Forms
            .Include(f => f.Sections)
            .FirstOrDefaultAsync(f => f.Id == id);
    }
    public async Task<List<Form>> GetAllAsync()
    {
        return await _context.Forms
            .Include(f => f.Sections)
            .ToListAsync();
    }
    public async Task<Form> CreateAsync(Form entity)
    {
        _context.Forms.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task<Form> UpdateAsync(Form entity)
    {
        _context.Forms.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task DeleteAsync(Guid id)
    {
        var form = await _context.Forms.FindAsync(id);
        if (form != null)
        {
            _context.Forms.Remove(form);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<Form> GetFormWithFieldsAsync(Guid id)
    {
        return await _context.Forms
            .Include(s => s.Sections)
            .FirstOrDefaultAsync(s => s.Id == id);
            
    }
    public async Task<Form> GetFormWithSectionsAndFieldsAsync(Guid id) {
        return await _context.Forms
            .Include(s => s.Sections)
                .ThenInclude(f => f.SectionFields)
            .FirstOrDefaultAsync(f => f.Id == id);
    }
    public async Task<Form> GetFormWithFieldsAndOptionsAsync(Guid id)
    {
        return await _context.Forms
            .Include(s => s.Sections)
            .ThenInclude(f => f.SectionFields)
                .ThenInclude(ff => ff.Options)
            .FirstOrDefaultAsync(f => f.Id == id);
    }
    public async Task<FormField> GetFormFieldWithOptionsAsync(Guid fieldId)
    {
        return await _context.FormFields
            .Include(ff => ff.Options)
            .FirstOrDefaultAsync(ff => ff.Id == fieldId);
    }
    public async Task<bool> FieldExistsInFormAsync(Guid formId, Guid fieldId)
    {
        return await _context.FormFields
            .AnyAsync(ff => ff.Id == fieldId && ff.FormId == formId);
    }
    public async Task AddFormFieldsAsync(Guid formId, IEnumerable<FormField> fields)
    {
        foreach (var field in fields)
        {
            field.FormId = formId; // Ensure the field is associated with the correct form
            _context.FormFields.Add(field);
        }
        await _context.SaveChangesAsync();
    }
    public async Task UpdateFormFieldsAsync(Guid formId, IEnumerable<FormField> fields)
    {
        foreach (var field in fields)
        {
            field.FormId = formId; // Ensure the field is associated with the correct form
            _context.FormFields.Update(field);
        }
        await _context.SaveChangesAsync();
    }
    public async Task RemoveFieldsAsync(Guid formId, Guid fieldId)
    {
        var field = await _context.FormFields
            .FirstOrDefaultAsync(ff => ff.Id == fieldId && ff.FormId == formId);
        if (field != null)
        {
            _context.FormFields.Remove(field);
            await _context.SaveChangesAsync();
        }
    }
    public async Task<bool> FormExistsAsync(Guid formId)
    {
        return await _context.Forms.AnyAsync(f => f.Id == formId);
    }
    public async Task<int> GetFormCountAsync()
    {
        return await _context.Forms.CountAsync();
    }
   
}