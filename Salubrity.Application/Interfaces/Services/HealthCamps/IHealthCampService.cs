using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.HealthCamps;

public interface IHealthCampService
{
    Task<List<HealthCampListDto>> GetAllAsync();
    Task<HealthCampDetailDto> GetByIdAsync(Guid id);

    Task<HealthCampDto> CreateAsync(CreateHealthCampDto dto);
    Task<HealthCampDto> UpdateAsync(Guid id, UpdateHealthCampDto dto);
    Task DeleteAsync(Guid id);
    Task<LaunchHealthCampResponseDto> LaunchAsync(LaunchHealthCampDto dto);


}
