using Salubrity.Application.DTOs.Camps;

namespace Salubrity.Application.Interfaces.Services.Camps;

public interface ICampService
{
    Task<List<CampDto>> GetAllAsync();
    Task<CampDto> GetByIdAsync(Guid id);
    Task<CampDto> CreateAsync(CreateCampDto dto);
    Task<CampDto> UpdateAsync(Guid id, UpdateCampDto dto);
    //Task DeleteAsync(Guid id);
}
