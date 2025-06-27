using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.Interfaces.Repositories.HealthcareServices;

public interface IServicePackageRepository
{
    Task<List<ServicePackage>> GetAllAsync();
    Task<ServicePackage?> GetByIdAsync(Guid id);
    Task<ServicePackage> AddAsync(ServicePackage entity);
    Task<ServicePackage> UpdateAsync(ServicePackage entity);
    Task DeleteAsync(ServicePackage entity);
    Task<bool> ExistsByNameAsync(string name);
}
