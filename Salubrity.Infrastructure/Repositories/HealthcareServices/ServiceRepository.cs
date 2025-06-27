using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.HealthcareServices;

public class ServiceRepository : IServiceRepository
{
    private readonly AppDbContext _db;

    public ServiceRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Service entity)
    {
        await _db.Services.AddAsync(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Service entity)
    {
        _db.Services.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Service>> GetAllAsync()
    {
        return await _db.Services.AsNoTracking().ToListAsync();
    }

    public async Task<Service?> GetByIdAsync(Guid id)
    {
        return await _db.Services.FindAsync(id);
    }

    public async Task UpdateAsync(Service entity)
    {
        _db.Services.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _db.Services.AnyAsync(x => x.Name == name);
    }
}
