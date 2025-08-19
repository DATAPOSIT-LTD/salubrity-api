using Salubrity.Application.DTOs.Rbac;

namespace Salubrity.Application.Interfaces.Rbac
{
    public interface IRolePermissionGroupService
    {
        Task<List<PermissionGroupDto>> GetPermissionGroupsByRoleAsync(Guid roleId);

        Task AssignPermissionGroupsToRoleAsync(AssignPermissionGroupsToRoleDto input);

        Task RemoveAllPermissionGroupsFromRoleAsync(Guid roleId);

        Task ReplacePermissionGroupsForRoleAsync(AssignPermissionGroupsToRoleDto input);
    }
}


