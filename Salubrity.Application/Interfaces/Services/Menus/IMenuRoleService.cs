using Salubrity.Application.DTOs.Menus;

namespace Salubrity.Application.Interfaces.Services.Menus
{
    public interface IMenuRoleService
    {
        /// <summary>
        /// Assigns roles to a menu (adds only new ones, ignores duplicates).
        /// </summary>
        Task AssignRolesAsync(MenuRoleCreateDto input);

        /// <summary>
        /// Completely replaces all roles assigned to a menu.
        /// </summary>
        Task ReplaceRolesAsync(Guid menuId, IEnumerable<Guid> roleIds);

        /// <summary>
        /// Gets all roles assigned to a specific menu.
        /// </summary>
        Task<List<MenuRoleResponseDto>> GetRolesForMenuAsync(Guid menuId);

        /// <summary>
        /// Removes a specific role from a menu.
        /// </summary>
        Task RemoveRoleAsync(Guid menuId, Guid roleId);

        /// <summary>
        /// Removes all roles from a menu.
        /// </summary>
        Task RemoveAllRolesAsync(Guid menuId);


        /// <summary>
        /// Gets all menus accessible by a specific role.
        /// </summary>
        Task<List<MenuResponseDto>> GetMenusByRoleAsync(Guid roleId);

    }
}
