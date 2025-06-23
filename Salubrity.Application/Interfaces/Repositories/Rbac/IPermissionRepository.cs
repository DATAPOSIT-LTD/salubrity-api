using Salubrity.Domain.Entities.Rbac;

namespace Salubrity.Application.Interfaces.Repositories.Rbac;


public interface IPermissionRepository
{
    Task<List<Permission>> GetAllPermissionsAsync();
    Task<Permission?> GetPermissionByIdAsync(Guid id);
    Task AddPermissionAsync(Permission permission);
    Task UpdatePermissionAsync(Permission permission);
    Task DeletePermissionAsync(Permission permission);
    Task<List<Permission>> GetPermissionsByRoleIdAsync(Guid roleId);
}
