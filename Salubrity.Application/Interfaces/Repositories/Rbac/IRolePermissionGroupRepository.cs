using Salubrity.Domain.Entities.Rbac;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Salubrity.Application.Interfaces.Repositories.Rbac

{
    public interface IRolePermissionGroupRepository
    {
        // CRUD Methods
        Task<List<RolePermissionGroup>> GetAllRolePermissionGroupsAsync();
        Task<RolePermissionGroup> GetRolePermissionGroupByIdAsync(Guid id);
        Task<RolePermissionGroup> CreateRolePermissionGroupAsync(RolePermissionGroup rolePermissionGroup);
        Task UpdateRolePermissionGroupAsync(RolePermissionGroup rolePermissionGroup);
        Task DeleteRolePermissionGroupAsync(RolePermissionGroup rolePermissionGroup);

        // RBAC-Specific Methods
        Task<List<PermissionGroup>> GetPermissionGroupsByRoleAsync(Guid roleId);
        Task AssignPermissionGroupToRoleAsync(Guid roleId, Guid permissionGroupId);
        Task UnassignAllPermissionGroupsAsync(Guid roleId);
    }
}
