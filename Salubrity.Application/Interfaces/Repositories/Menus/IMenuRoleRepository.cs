using Salubrity.Domain.Entities.Menus;
using Salubrity.Domain.Entities.Rbac;

namespace Salubrity.Application.Interfaces.Repositories.Menus;

public interface IMenuRoleRepository
{
    Task ReplaceRolesForMenuAsync(Guid menuId, IEnumerable<Guid> roleIds);
    Task<List<Role>> GetRolesForMenuAsync(Guid menuId);
    Task<bool> RemoveRoleAsync(Guid menuId, Guid roleId);

    Task RemoveAllRolesFromMenuAsync(Guid menuId);
    Task<List<Menu>> GetMenusByRoleAsync(Guid roleId);
    Task<List<Guid>> GetMenuIdsByRoleAsync(Guid roleId);
}
