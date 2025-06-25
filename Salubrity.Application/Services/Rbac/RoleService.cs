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
            .OrderBy(r => r.Order) // Order by Order property
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                IconClass = r.IconClass,
                // Add Order to DTO if needed
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
            IconClass = role.IconClass,
            // Add Order to DTO if needed
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
            IconClass = input.IconClass,
            Order = input.Order // assuming you add Order to CreateRoleDto
        };

        await _repository.AddRoleAsync(role);

        var dto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IconClass = role.IconClass,
            Order = role.Order
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
        role.IconClass = input.IconClass ?? role.IconClass;
        role.Order = input.Order;

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
