using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Menus;
using Salubrity.Domain.Entities.Menus;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Menus;

public class MenuRoleRepository : IMenuRoleRepository
{
    private readonly AppDbContext _db;

    public MenuRoleRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task ReplaceRolesForMenuAsync(Guid menuId, IEnumerable<Guid> roleIds)
    {
        var existing = _db.MenuRoles.Where(x => x.MenuId == menuId);
        _db.MenuRoles.RemoveRange(existing);

        var newLinks = roleIds.Select(roleId => new MenuRole
        {
            MenuId = menuId,
            RoleId = roleId
        });

        await _db.MenuRoles.AddRangeAsync(newLinks);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Role>> GetRolesForMenuAsync(Guid menuId)
    {
        return await _db.MenuRoles
            .Where(mr => mr.MenuId == menuId)
            .Include(mr => mr.Role)
            .Select(mr => mr.Role)
            .ToListAsync();
    }

    public async Task<bool> RemoveRoleAsync(Guid menuId, Guid roleId)
    {
        var entity = await _db.MenuRoles
            .FirstOrDefaultAsync(x => x.MenuId == menuId && x.RoleId == roleId);

        if (entity == null) return false;

        _db.MenuRoles.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task RemoveAllRolesFromMenuAsync(Guid menuId)
    {
        var links = _db.MenuRoles.Where(x => x.MenuId == menuId);
        _db.MenuRoles.RemoveRange(links);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Menu>> GetMenusByRoleAsync(Guid roleId)
    {
        return await _db.MenuRoles
            .Where(x => x.RoleId == roleId)
            .Include(x => x.Menu)
            .Select(x => x.Menu)
            .ToListAsync();
    }

    public async Task<List<Guid>> GetMenuIdsByRoleAsync(Guid roleId)
    {
        return await _db.MenuRoles
            .Where(x => x.RoleId == roleId)
            .Select(x => x.MenuId)
            .ToListAsync();
    }
}
