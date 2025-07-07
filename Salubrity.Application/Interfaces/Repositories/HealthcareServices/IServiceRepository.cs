using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.Interfaces.Repositories.HealthcareServices;

public interface IServiceRepository
{
	Task<List<Service>> GetAllAsync();
	Task<Service?> GetByIdAsync(Guid id);
	Task AddAsync(Service entity);
	Task UpdateAsync(Service entity);
	Task DeleteAsync(Service entity);
	Task<bool> ExistsByNameAsync(string name);
    Task<bool> ExistsByIdAsync(Guid id);
}
