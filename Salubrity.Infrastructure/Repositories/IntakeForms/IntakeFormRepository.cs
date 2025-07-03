using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.IntakeForms;

public class IntakeFormRepository : IIntakeFormRepository
{
    private readonly AppDbContext _db;

    public IntakeFormRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<IntakeForm>> GetAllAsync()
    {
        return await _db.IntakeForms.AsNoTracking().ToListAsync();
    }

    public async Task<IntakeForm?> GetByIdAsync(Guid id)
    {
        return await _db.IntakeForms.FindAsync(id);
    }

    public async Task AddAsync(IntakeForm form)
    {
        await _db.IntakeForms.AddAsync(form);
    }

    public async Task UpdateAsync(IntakeForm form)
    {
        _db.IntakeForms.Update(form);
    }

    public async Task DeleteAsync(IntakeForm form)
    {
        _db.IntakeForms.Remove(form);
    }
}
