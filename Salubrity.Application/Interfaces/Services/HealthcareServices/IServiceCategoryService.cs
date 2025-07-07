using Salubrity.Application.DTOs.HealthcareServices;

namespace Salubrity.Application.Interfaces.Services.HealthcareServices;


public interface IServiceCategoryService
{
    Task<List<ServiceCategoryResponseDto>> GetAllAsync();
    Task<ServiceCategoryResponseDto> GetByIdAsync(Guid id);
    Task<ServiceCategoryResponseDto> CreateAsync(CreateServiceCategoryDto dto);
    Task<ServiceCategoryResponseDto> UpdateAsync(Guid id, UpdateServiceCategoryDto dto);
    Task DeleteAsync(Guid id);
    //Task<bool> ExistsByIdAsync(Guid id);
}
