using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HealthcareServices;

public class ServiceCategoryRepository : IServiceCategoryRepository
{
    private readonly AppDbContext _db;

    public ServiceCategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ServiceCategory>> GetAllAsync()
    {
        return await _db.ServiceCategories
            .Include(c => c.Service)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<ServiceCategory?> GetByIdAsync(Guid id)
    {
        return await _db.ServiceCategories
            .Include(c => c.Service)
            .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
    }

    public async Task<ServiceCategory?> GetByNameAsync(string name)
    {
        return await _db.ServiceCategories
            .Include(c => c.Service)
            .FirstOrDefaultAsync(c => c.Name == name && !c.IsDeleted);
    }

    public async Task AddAsync(ServiceCategory entity)
    {
        await _db.ServiceCategories.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(ServiceCategory entity)
    {
        _db.ServiceCategories.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ServiceCategory entity)
    {
        _db.ServiceCategories.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsByIdAsync(Guid id)
    {
        return await _db.ServiceCategories.AnyAsync(s => s.Id == id);
    }

    public async Task<ServiceCategory?> GetByIdWithSubcategoriesAsync(Guid id)
    {
        return await _db.ServiceCategories
            .Include(c => c.Subcategories)
            .Include(c => c.Service)
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
    }

    public async Task<bool> IsNameUniqueAsync(string name, CancellationToken ct = default)
    {
        return !await _db.ServiceCategories
            .AnyAsync(c => c.Name == name, ct);
    }
}
