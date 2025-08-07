using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.DTOs.HealthcareServices;

namespace Salubrity.Infrastructure.Repositories.HealthcareServices;

public class IndustryRepository : IIndustryRepository
{
    private readonly AppDbContext _db;

    public IndustryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Industry entity)
    {
        await _db.Industries.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Industry entity)
    {
        _db.Industries.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Industry>> GetAllAsync()
    {
        return await _db.Industries.AsNoTracking().ToListAsync();
    }

    public async Task<Industry?> GetByIdAsync(Guid id)
    {
        return await _db.Industries.FindAsync(id);
    }

    public async Task UpdateAsync(Industry entity)
    {
        _db.Industries.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _db.Industries.AnyAsync(i => i.Name == name);
    }



    public async Task<Industry?> GetByNameAsync(string name)
    {
        var industry = await _db.Industries
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Name == name && !i.IsDeleted);

        return industry;
    }

}
