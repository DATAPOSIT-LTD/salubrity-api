using Salubrity.Application.DTOs.Rbac;
using Salubrity.Shared.Responses;

namespace Salubrity.Application.Interfaces.Rbac;

public interface IRoleService
{
    Task<ApiResponse<IEnumerable<RoleDto>>> GetAllAsync();
    Task<ApiResponse<RoleDto>> GetByIdAsync(Guid id);
    Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleDto input);
    Task<ApiResponse<string>> UpdateAsync(Guid id, UpdateRoleDto input);
    Task<ApiResponse<string>> DeleteAsync(Guid id);
}
