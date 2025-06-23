using Salubrity.Application.DTOs.Rbac;


namespace Salubrity.Application.Interfaces.Rbac;

public interface IPermissionGroupService
{
    Task<List<PermissionGroupDto>> GetAllAsync();
    Task<PermissionGroupDto> GetByIdAsync(Guid id);
    Task<PermissionGroupDto> CreateAsync(CreatePermissionGroupDto input);
    Task UpdateAsync(Guid id, UpdatePermissionGroupDto input);
    Task DeleteAsync(Guid id);
}
