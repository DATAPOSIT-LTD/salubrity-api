using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.Interfaces.Repositories.HealthcareServices;

public interface IServiceSubcategoryRepository
{
    Task<List<ServiceSubcategory>> GetAllAsync();
    Task<ServiceSubcategory?> GetByIdAsync(Guid id);
    Task AddAsync(ServiceSubcategory entity);
    Task UpdateAsync(ServiceSubcategory entity);
    Task DeleteAsync(ServiceSubcategory entity);
    Task<bool> ExistsByNameAsync(string name);
}
