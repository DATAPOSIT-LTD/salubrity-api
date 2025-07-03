using Salubrity.Application.DTOs.IntakeForms;

namespace Salubrity.Application.Interfaces.IntakeForms;

public interface IIntakeFormService
{
    Task<List<IntakeFormDto>> GetAllAsync();
    Task<IntakeFormDto?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(CreateIntakeFormDto dto);
    Task<bool> UpdateAsync(Guid id, UpdateIntakeFormDto dto);
    Task<bool> DeleteAsync(Guid id);
}
