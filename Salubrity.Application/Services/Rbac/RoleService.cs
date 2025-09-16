using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Shared.Exceptions;
using Salubrity.Shared.Responses;

namespace Salubrity.Application.Services.Rbac;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _repository;

    public RoleService(IRoleRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<IEnumerable<RoleDto>>> GetAllAsync()
    {
        var roles = await _repository.GetAllRolesAsync();

        var result = roles
            .Where(r =>
                string.Equals(r.Name, "Patient", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r.Name, "Subcontractor", StringComparison.OrdinalIgnoreCase)
            )
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IsActive = r.IsActive,
                IsGlobal = r.IsGlobal,
                OrganizationId = r.OrganizationId
            });

        return ApiResponse<IEnumerable<RoleDto>>.CreateSuccess(result);
    }




    public async Task<ApiResponse<RoleDto>> GetByIdAsync(Guid id)
    {
        var role = (await _repository.GetAllRolesAsync())
            .FirstOrDefault(r => r.Id == id);

        if (role == null)
            throw new NotFoundException("Role not found.");

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,

        };

        return ApiResponse<RoleDto>.CreateSuccess(dto);
    }

    public async Task<ApiResponse<RoleDto>> CreateAsync(CreateRoleDto input)
    {
        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            Description = input.Description,

        };

        await _repository.AddRoleAsync(role);

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,

        };

        return ApiResponse<RoleDto>.CreateSuccess(dto, "Role created successfully.");
    }

    public async Task<ApiResponse<string>> UpdateAsync(Guid id, UpdateRoleDto input)
    {
        var role = (await _repository.GetAllRolesAsync())
            .FirstOrDefault(r => r.Id == id);

        if (role == null)
            throw new NotFoundException("Role not found.");

        role.Name = input.Name ?? role.Name;
        role.Description = input.Description ?? role.Description;


        await _repository.UpdateRoleAsync(role);

        return ApiResponse<string>.CreateSuccessMessage("Role updated successfully.");
    }

    public async Task<ApiResponse<string>> DeleteAsync(Guid id)
    {
        var role = (await _repository.GetAllRolesAsync())
            .FirstOrDefault(r => r.Id == id);

        if (role == null)
            throw new NotFoundException("Role not found.");

        await _repository.DeleteRoleAsync(role);

        return ApiResponse<string>.CreateSuccessMessage("Role deleted successfully.");
    }
}
