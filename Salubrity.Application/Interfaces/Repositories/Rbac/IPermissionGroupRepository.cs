using Salubrity.Domain.Entities.Rbac;

namespace Salubrity.Application.Interfaces.Repositories.Rbac;


public interface IPermissionGroupRepository
{
    Task<List<PermissionGroup>> GetAllAsync();
    Task<PermissionGroup?> GetByIdAsync(Guid id);
    Task AddAsync(PermissionGroup group);
    Task UpdateAsync(PermissionGroup group);
    Task DeleteAsync(PermissionGroup group);
}

