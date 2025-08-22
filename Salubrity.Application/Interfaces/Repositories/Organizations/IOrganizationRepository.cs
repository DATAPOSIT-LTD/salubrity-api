using Salubrity.Domain.Entities.Organizations;

namespace Salubrity.Application.Interfaces.Repositories.Organizations
{
    public interface IOrganizationRepository
    {
        Task<Organization> CreateAsync(Organization entity);
        Task<Organization?> GetByIdAsync(Guid id);
        Task<List<Organization>> GetAllAsync();
        Task UpdateAsync(Organization entity);
        Task DeleteAsync(Guid id);
        Task<Organization?> FindByNameAsync(string name);

    }
}
