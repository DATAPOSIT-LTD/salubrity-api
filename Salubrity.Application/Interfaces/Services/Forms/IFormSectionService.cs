using Salubrity.Application.DTOs.Forms;

namespace Salubrity.Application.Interfaces.Services.Forms;

public interface IFormSectionService
{
    Task<FormSectionResponseDto> CreateAsync(Guid formId, CreateFormSectionDto dto);
    Task<List<FormSectionResponseDto>> GetByFormIdAsync(Guid formId);
    Task<FormSectionResponseDto> GetByIdAsync(Guid formId, Guid sectionId);
    Task<FormSectionResponseDto> UpdateAsync(Guid formId, Guid sectionId, UpdateFormSectionDto dto);
    Task<bool> DeleteAsync(Guid formId, Guid sectionId);
}
