// File: Salubrity.Infrastructure/Repositories/Forms/FormRepository.cs

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

    public async Task<Form?> GetByIdAsync(Guid id)
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
        if (form is not null)
        {
            _context.Forms.Remove(form);
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

    //  Implements: GetWithSectionsAsync — full tree: Sections -> Fields -> Options
    public async Task<Form?> GetWithSectionsAsync(Guid formId)
    {
        return await _context.Forms
            .Include(f => f.Sections)
                .ThenInclude(s => s.SectionFields)
                    .ThenInclude(f => f.Options)
            .FirstOrDefaultAsync(f => f.Id == formId);
    }

    //  Implements: GetWithFieldsAsync — direct fields on form (no sections)
    public async Task<Form?> GetWithFieldsAsync(Guid formId)
    {
        return await _context.Forms
            .Include(f => f.Fields)
                .ThenInclude(ff => ff.Options)
            .FirstOrDefaultAsync(f => f.Id == formId);
    }
}
