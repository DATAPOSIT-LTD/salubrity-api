using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Application.Interfaces.Repositories.Rbac;

namespace Salubrity.Infrastructure.Repositories.Rbac;


public class PermissionGroupRepository : IPermissionGroupRepository
{
    private readonly AppDbContext _db;

    public PermissionGroupRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<PermissionGroup>> GetAllAsync()
    {
        return await _db.PermissionGroups.ToListAsync();
    }

    public async Task<PermissionGroup?> GetByIdAsync(Guid id)
    {
        return await _db.PermissionGroups.FindAsync(id);
    }

    public async Task AddAsync(PermissionGroup group)
    {
        await _db.PermissionGroups.AddAsync(group);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(PermissionGroup group)
    {
        _db.PermissionGroups.Update(group);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(PermissionGroup group)
    {
        _db.PermissionGroups.Remove(group);
        await _db.SaveChangesAsync();
    }
}
