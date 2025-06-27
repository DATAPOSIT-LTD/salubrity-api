using Salubrity.Application.DTOs.HealthcareServices;

namespace Salubrity.Application.Interfaces.Services.HealthcareServices;

public interface IServicePackageService
{
    Task<List<ServicePackageResponseDto>> GetAllAsync();
    Task<ServicePackageResponseDto> GetByIdAsync(Guid id);
    Task<ServicePackageResponseDto> CreateAsync(CreateServicePackageDto dto);
    Task<ServicePackageResponseDto> UpdateAsync(Guid id, UpdateServicePackageDto dto);
    Task DeleteAsync(Guid id);
}
