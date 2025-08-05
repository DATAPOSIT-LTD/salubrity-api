using Salubrity.Application.DTOs.Forms;

namespace Salubrity.Application.Interfaces.Services.Forms;

public interface IFormService
{
    Task<FormResponseDto> CreateAsync(CreateFormDto dto);
    Task<List<FormResponseDto>> GetAllAsync();
    Task<FormResponseDto> GetByIdAsync(Guid id);
    Task<FormResponseDto> UpdateAsync(Guid id, UpdateFormDto dto);
    Task DeleteAsync(Guid id);
}
