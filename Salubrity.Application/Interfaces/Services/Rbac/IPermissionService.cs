using Salubrity.Application.DTOs.Rbac;

namespace Salubrity.Application.Interfaces.Rbac;

public interface IPermissionService
{
    Task<List<PermissionDto>> GetAllPermissionsAsync();
    Task<PermissionDto> GetPermissionByIdAsync(Guid id);
    Task<PermissionDto> CreatePermissionAsync(PermissionCreateDto input);
    Task<PermissionDto> UpdatePermissionAsync(Guid id, PermissionUpdateDto input);
    Task DeletePermissionAsync(Guid id);

    /// <summary>
    /// Retrieves all permissions assigned to a specific role.
    /// </summary>
    Task<List<PermissionDto>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
}
