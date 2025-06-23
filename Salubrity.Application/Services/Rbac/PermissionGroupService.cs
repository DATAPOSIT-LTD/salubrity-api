using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Rbac;

public class PermissionGroupService : IPermissionGroupService
{
    private readonly IPermissionGroupRepository _repo;

    public PermissionGroupService(IPermissionGroupRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<PermissionGroupDto>> GetAllAsync()
    {
        var groups = await _repo.GetAllAsync();
        return groups.Select(g => new PermissionGroupDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            CreatedAt = g.CreatedAt,
            UpdatedAt = g.UpdatedAt
        }).ToList();
    }

    public async Task<PermissionGroupDto> GetByIdAsync(Guid id)
    {
        var group = await _repo.GetByIdAsync(id);
        if (group is null)
            throw new NotFoundException("Permission group not found.");

        return new PermissionGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            CreatedAt = group.CreatedAt,
            UpdatedAt = group.UpdatedAt
        };
    }

    public async Task<PermissionGroupDto> CreateAsync(CreatePermissionGroupDto input)
    {
        var entity = new PermissionGroup
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            Description = input.Description
        };

        await _repo.AddAsync(entity);

        return new PermissionGroupDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task UpdateAsync(Guid id, UpdatePermissionGroupDto input)
    {
        var group = await _repo.GetByIdAsync(id);
        if (group is null)
            throw new NotFoundException("Permission group not found.");

        group.Name = input.Name;
        group.Description = input.Description;
        await _repo.UpdateAsync(group);
    }

    public async Task DeleteAsync(Guid id)
    {
        var group = await _repo.GetByIdAsync(id);
        if (group is null)
            throw new NotFoundException("Permission group not found.");

        await _repo.DeleteAsync(group);
    }
}
