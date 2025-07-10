using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampRepository
{
    Task<List<HealthCampListDto>> GetAllAsync();
    Task<HealthCamp?> GetByIdAsync(Guid id);
    Task<HealthCampDetailDto?> GetCampDetailsByIdAsync(Guid id); 
    Task<HealthCamp> CreateAsync(HealthCamp entity);
    Task<HealthCamp> UpdateAsync(HealthCamp entity);
    Task DeleteAsync(Guid id);
 
}
