using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampRepository
{
    Task<List<Camp>> GetAllAsync();
    Task<Camp?> GetByIdAsync(Guid id);
    Task<Camp> CreateAsync(Camp entity);
    Task<Camp> UpdateAsync(Camp entity);
    Task DeleteAsync(Guid id);
}
