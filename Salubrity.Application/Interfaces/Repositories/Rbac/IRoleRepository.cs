using Salubrity.Domain.Entities.Rbac;

namespace Salubrity.Application.Interfaces.Repositories.Rbac;


public interface IRoleRepository
{
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role?> GetByIdAsync(Guid id);
    Task AddRoleAsync(Role role);
    Task UpdateRoleAsync(Role role);
    Task DeleteRoleAsync(Role role);
    Task FindByNameAsync(string roleName);
}
