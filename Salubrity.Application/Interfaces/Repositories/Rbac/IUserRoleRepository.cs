using Salubrity.Domain.Entities.Rbac;


namespace Salubrity.Application.Interfaces.Repositories.Rbac;


public interface IUserRoleRepository
{
    Task<List<UserRole>> GetAllAsync();
    Task<UserRole?> GetByIdAsync(Guid id);
    Task AddAsync(UserRole userRole);
    Task UpdateAsync(UserRole userRole);
    Task DeleteAsync(UserRole userRole);
}
