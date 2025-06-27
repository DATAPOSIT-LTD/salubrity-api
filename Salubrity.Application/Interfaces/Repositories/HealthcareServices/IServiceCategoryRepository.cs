using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.Interfaces.Repositories.HealthcareServices;

public interface IServiceCategoryRepository
{
    Task<List<ServiceCategory>> GetAllAsync();
    Task<ServiceCategory?> GetByIdAsync(Guid id);
    Task AddAsync(ServiceCategory entity);
    Task UpdateAsync(ServiceCategory entity);
    Task DeleteAsync(ServiceCategory entity);
}
