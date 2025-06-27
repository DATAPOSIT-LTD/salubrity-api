using Salubrity.Application.DTOs.HealthcareServices;

namespace Salubrity.Application.Interfaces.Services.HealthcareServices;

public interface IServiceService
{
    Task<List<ServiceResponseDto>> GetAllAsync();
    Task<ServiceResponseDto> GetByIdAsync(Guid id);
    Task<ServiceResponseDto> CreateAsync(CreateServiceDto input);
    Task<ServiceResponseDto> UpdateAsync(Guid id, UpdateServiceDto input);
    Task DeleteAsync(Guid id);
}
