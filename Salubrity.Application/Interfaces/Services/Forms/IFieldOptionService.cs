using Salubrity.Application.DTOs.Forms;

namespace Salubrity.Application.Interfaces.Services.Forms;

public interface IFieldOptionService
{
    Task<FieldOptionResponseDto> CreateAsync(Guid formId, Guid fieldId, CreateFieldOptionDto dto);
    Task<List<FieldOptionResponseDto>> GetByFieldIdAsync(Guid formId, Guid fieldId);
}
