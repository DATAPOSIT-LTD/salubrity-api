using Microsoft.EntityFrameworkCore;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Infrastructure.Persistence;
using Salubrity.Application.Interfaces.Repositories.Rbac;


namespace Salubrity.Infrastructure.Repositories.Rbac;

public class PermissionRepository : IPermissionRepository
{
    private readonly AppDbContext _context;

    public PermissionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Permission>> GetAllPermissionsAsync()
    {
        return await _context.Permissions
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Permission?> GetPermissionByIdAsync(Guid id)
    {
        return await _context.Permissions.FindAsync(id);
    }

    public async Task AddPermissionAsync(Permission permission)
    {
        await _context.Permissions.AddAsync(permission);
        await _context.SaveChangesAsync();
    }

    public async Task UpdatePermissionAsync(Permission permission)
    {
        _context.Permissions.Update(permission);
        await _context.SaveChangesAsync();
    }

    public async Task DeletePermissionAsync(Permission permission)
    {
        _context.Permissions.Remove(permission);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Permission>> GetPermissionsByRoleIdAsync(Guid roleId)
    {
        return await _context.RolePermissionGroups
            .Where(rpg => rpg.RoleId == roleId)
            .Join(
                _context.PermissionGroupPermissions,
                rpg => rpg.PermissionGroupId,
                pgp => pgp.PermissionGroupId,
                (rpg, pgp) => pgp.Permission
            )
            .Distinct()
            .ToListAsync();
    }
}
