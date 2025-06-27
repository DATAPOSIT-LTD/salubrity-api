using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HealthcareServices;

public class ServicePackageRepository : IServicePackageRepository
{
    private readonly AppDbContext _db;

    public ServicePackageRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ServicePackage>> GetAllAsync()
        => await _db.ServicePackages.AsNoTracking().ToListAsync();

    public async Task<ServicePackage?> GetByIdAsync(Guid id)
        => await _db.ServicePackages.FindAsync(id);

    public async Task<ServicePackage> AddAsync(ServicePackage entity)
    {
        await _db.ServicePackages.AddAsync(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task<ServicePackage> UpdateAsync(ServicePackage entity)
    {
        _db.ServicePackages.Update(entity);
        await _db.SaveChangesAsync();
        return entity;
    }

    public async Task DeleteAsync(ServicePackage entity)
    {
        _db.ServicePackages.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
        => await _db.ServicePackages.AnyAsync(x => x.Name == name);
}
