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
        => await _db.ServiceCategories.AsNoTracking().ToListAsync();

    public async Task<ServiceCategory?> GetByIdAsync(Guid id)
        => await _db.ServiceCategories.FindAsync(id);

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
}
