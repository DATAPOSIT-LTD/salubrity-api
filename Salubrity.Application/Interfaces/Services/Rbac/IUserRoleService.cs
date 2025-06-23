using Salubrity.Application.DTOs.Rbac;

namespace Salubrity.Application.Interfaces.Rbac;

public interface IUserRoleService
{
    Task<List<UserRoleDto>> GetAllUserRolesAsync();
    Task<UserRoleDto> GetUserRoleByIdAsync(Guid id);
    Task<UserRoleDto> CreateUserRoleAsync(CreateUserRoleDto input);
    Task UpdateUserRoleAsync(Guid id, UpdateUserRoleDto input);
    Task DeleteUserRoleAsync(Guid id);
}
