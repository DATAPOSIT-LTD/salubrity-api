using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HealthcareServices;

public class ServiceSubcategoryRepository : IServiceSubcategoryRepository
{
    private readonly AppDbContext _db;

    public ServiceSubcategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ServiceSubcategory>> GetAllAsync()
    {
        return await _db.ServiceSubcategories
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceSubcategory?> GetByIdAsync(Guid id)
    {
        return await _db.ServiceSubcategories.FindAsync(id);
    }

    public async Task AddAsync(ServiceSubcategory entity)
    {
        await _db.ServiceSubcategories.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ServiceSubcategory entity)
    {
        _db.ServiceSubcategories.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ServiceSubcategory entity)
    {
        _db.ServiceSubcategories.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _db.ServiceSubcategories.AnyAsync(s => s.Name == name);
    }
}
