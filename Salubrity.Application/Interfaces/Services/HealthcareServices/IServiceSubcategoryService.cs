using Salubrity.Application.DTOs.HealthcareServices;

namespace Salubrity.Application.Interfaces.Services.HealthcareServices;

public interface IServiceSubcategoryService
{
	Task<List<ServiceSubcategoryResponseDto>> GetAllAsync();
	Task<ServiceSubcategoryResponseDto> GetByIdAsync(Guid id);
	Task<ServiceSubcategoryResponseDto> CreateAsync(CreateServiceSubcategoryDto dto);
	Task<ServiceSubcategoryResponseDto> UpdateAsync(Guid id, UpdateServiceSubcategoryDto dto);
	Task DeleteAsync(Guid id);
}
