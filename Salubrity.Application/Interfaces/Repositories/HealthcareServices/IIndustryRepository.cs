using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.Interfaces.Repositories.HealthcareServices;

public interface IIndustryRepository
{
    Task<List<Industry>> GetAllAsync();
    Task<Industry?> GetByIdAsync(Guid id);
    Task AddAsync(Industry entity);
    Task UpdateAsync(Industry entity);
    Task DeleteAsync(Industry entity);
    Task<bool> ExistsByNameAsync(string name);
    Task<Industry> GetByNameAsync(string name);
}
