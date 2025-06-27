using Salubrity.Domain.Entities.Camps;

namespace Salubrity.Application.Interfaces.Repositories.Camps;

public interface ICampRepository
{
    Task<List<Camp>> GetAllAsync();
    Task<Camp?> GetByIdAsync(Guid id);
    Task<Camp> CreateAsync(Camp entity);
    Task<Camp> UpdateAsync(Camp entity);
    Task DeleteAsync(Guid id);
}
