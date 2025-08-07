using Salubrity.Application.DTOs.HealthcareServices;

namespace Salubrity.Application.Interfaces.Services.HealthcareServices;


public interface IIndustryService
{
    Task<List<IndustryResponseDto>> GetAllAsync();
    Task<IndustryResponseDto> GetByIdAsync(Guid id);
    Task<IndustryResponseDto> CreateAsync(CreateIndustryDto dto);
    Task<IndustryResponseDto> UpdateAsync(Guid id, UpdateIndustryDto dto);
    Task DeleteAsync(Guid id);
    Task<IndustryResponseDto> GetDefaultIndustryAsync();
}
