using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Rbac
{
    public class RolePermissionGroupService : IRolePermissionGroupService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IPermissionGroupRepository _permissionGroupRepository;
        private readonly IRolePermissionGroupRepository _rolePermissionGroupRepository;

        public RolePermissionGroupService(
            IRoleRepository roleRepository,
            IPermissionGroupRepository permissionGroupRepository,
            IRolePermissionGroupRepository rolePermissionGroupRepository)
        {
            _roleRepository = roleRepository;
            _permissionGroupRepository = permissionGroupRepository;
            _rolePermissionGroupRepository = rolePermissionGroupRepository;
        }

        public async Task<List<PermissionGroupDto>> GetPermissionGroupsByRoleAsync(Guid roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId)
                       ?? throw new NotFoundException("Role not found");

            var groups = await _rolePermissionGroupRepository.GetPermissionGroupsByRoleAsync(roleId);

            return [.. groups.Select(g => new PermissionGroupDto
            {
                Id = g.Id,
                Name = g.Name
            })];
        }

        public async Task AssignPermissionGroupsToRoleAsync(AssignPermissionGroupsToRoleDto input)
        {
            var role = await _roleRepository.GetByIdAsync(input.RoleId)
                       ?? throw new NotFoundException("Role not found");

            foreach (var groupId in input.PermissionGroupIds.Distinct())
            {
                await _rolePermissionGroupRepository.AssignPermissionGroupToRoleAsync(input.RoleId, groupId);
            }
        }

        public async Task RemoveAllPermissionGroupsFromRoleAsync(Guid roleId)
        {
            var role = await _roleRepository.GetByIdAsync(roleId)
                       ?? throw new NotFoundException("Role not found");

            await _rolePermissionGroupRepository.UnassignAllPermissionGroupsAsync(roleId);
        }

        public async Task ReplacePermissionGroupsForRoleAsync(AssignPermissionGroupsToRoleDto input)
        {
            var role = await _roleRepository.GetByIdAsync(input.RoleId)
                       ?? throw new NotFoundException("Role not found");

            await _rolePermissionGroupRepository.UnassignAllPermissionGroupsAsync(input.RoleId);

            foreach (var groupId in input.PermissionGroupIds.Distinct())
            {
                await _rolePermissionGroupRepository.AssignPermissionGroupToRoleAsync(input.RoleId, groupId);
            }
        }
    }
}
