using Salubrity.Application.DTOs.Rbac;
using Salubrity.Application.Interfaces.Rbac;
using Salubrity.Application.Interfaces.Repositories.Rbac;
using Salubrity.Domain.Entities.Rbac;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Rbac;

public class UserRoleService : IUserRoleService
{
    private readonly IUserRoleRepository _repo;

    public UserRoleService(IUserRoleRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<UserRoleDto>> GetAllUserRolesAsync()
    {
        var data = await _repo.GetAllAsync();
        return data.Select(x => new UserRoleDto
        {
            Id = x.Id,
            UserId = x.UserId,
            RoleId = x.RoleId,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToList();
    }

    public async Task<UserRoleDto> GetUserRoleByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null)
            throw new NotFoundException("UserRole not found.");

        return new UserRoleDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RoleId = entity.RoleId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task<UserRoleDto> CreateUserRoleAsync(CreateUserRoleDto input)
    {
        var entity = new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = input.UserId,
            RoleId = input.RoleId
        };

        await _repo.AddAsync(entity);

        return new UserRoleDto
        {
            Id = entity.Id,
            UserId = entity.UserId,
            RoleId = entity.RoleId,
            CreatedAt = entity.CreatedAt
        };
    }

    public async Task UpdateUserRoleAsync(Guid id, UpdateUserRoleDto input)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null)
            throw new NotFoundException("UserRole not found.");

        entity.UserId = input.UserId;
        entity.RoleId = input.RoleId;

        await _repo.UpdateAsync(entity);
    }

    public async Task DeleteUserRoleAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id);
        if (entity is null)
            throw new NotFoundException("UserRole not found.");

        await _repo.DeleteAsync(entity);
    }
}
