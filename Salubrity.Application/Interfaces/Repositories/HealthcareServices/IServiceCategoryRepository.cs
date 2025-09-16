using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.Interfaces.Repositories.HealthcareServices;

public interface IServiceCategoryRepository
{
    Task<List<ServiceCategory>> GetAllAsync();
    Task<ServiceCategory?> GetByIdAsync(Guid id);
    Task AddAsync(ServiceCategory entity);
    Task UpdateAsync(ServiceCategory entity);
    Task DeleteAsync(ServiceCategory entity);
    Task<bool> ExistsByIdAsync(Guid id);
    Task<ServiceCategory?> GetByIdWithSubcategoriesAsync(Guid categoryId);
    Task<bool> IsNameUniqueAsync(string name, CancellationToken ct = default);
    Task<ServiceCategory?> GetByNameAsync(string name);
}
