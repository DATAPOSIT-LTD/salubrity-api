using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Rbac;

public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;

    public PermissionService(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<List<PermissionDto>> GetAllPermissionsAsync()
    {
        var permissions = await _permissionRepository.GetAllPermissionsAsync();
        return [.. permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Code = p.Code,
            Description = p.Description,
            Name = p.GetPermissionName()
        })];
    }

    public async Task<PermissionDto> GetPermissionByIdAsync(Guid id)
    {
        var permission = await _permissionRepository.GetPermissionByIdAsync(id)
            ?? throw new NotFoundException("Permission not found.");

        return new PermissionDto
        {
            Id = permission.Id,
            Code = permission.Code,
            Description = permission.Description,
            Name = permission.GetPermissionName()
        };
    }

    public async Task<PermissionDto> CreatePermissionAsync(PermissionCreateDto input)
    {
        var entity = new Permission
        {
            Id = Guid.NewGuid(),
            Code = input.Code,
            Description = input.Description
        };

        await _permissionRepository.AddPermissionAsync(entity);

        return new PermissionDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Description = entity.Description,
            Name = entity.GetPermissionName()
        };
    }

    public async Task<PermissionDto> UpdatePermissionAsync(Guid id, PermissionUpdateDto input)
    {
        var entity = await _permissionRepository.GetPermissionByIdAsync(id)
            ?? throw new NotFoundException("Permission not found.");

        entity.Code = input.Code;
        entity.Description = input.Description;

        await _permissionRepository.UpdatePermissionAsync(entity);

        return new PermissionDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Description = entity.Description,
            Name = entity.GetPermissionName()
        };
    }

    public async Task DeletePermissionAsync(Guid id)
    {
        var entity = await _permissionRepository.GetPermissionByIdAsync(id)
            ?? throw new NotFoundException("Permission not found.");

        await _permissionRepository.DeletePermissionAsync(entity);
    }

    public async Task<List<PermissionDto>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var permissions = await _permissionRepository.GetPermissionsByRoleIdAsync(roleId);

        return [.. permissions.Select(p => new PermissionDto
        {
            Id = p.Id,
            Code = p.Code,
            Description = p.Description,
            Name = p.GetPermissionName()
        })];
    }
}
