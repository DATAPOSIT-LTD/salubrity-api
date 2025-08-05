using Salubrity.Application.DTOs.Forms;

namespace Salubrity.Application.Interfaces.Services.Forms;

public interface IFormFieldService
{
    Task<FormFieldResponseDto> CreateAsync(Guid formId, Guid sectionId, CreateFormFieldDto dto);
    Task<List<FormFieldResponseDto>> GetByFormIdAsync(Guid formId);
    Task<FormFieldResponseDto> GetByIdAsync(Guid formId, Guid fieldId);
    Task<FormFieldResponseDto> UpdateAsync(Guid formId, Guid fieldId, UpdateFormFieldDto dto);
    Task<bool> DeleteAsync(Guid formId, Guid fieldId);
}
