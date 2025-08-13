// File: Salubrity.Application/Interfaces/Repositories/Forms/IFormSectionRepository.cs

using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Interfaces.Repositories.IntakeForms;

public interface IFormSectionRepository
{
    Task<IntakeFormSection?> GetByIdAsync(Guid sectionId);
    Task<List<IntakeFormSection>> GetByFormIdAsync(Guid formId);
    Task<IntakeFormSection> CreateAsync(IntakeFormSection section);
    Task<IntakeFormSection> UpdateAsync(IntakeFormSection section);
    Task DeleteAsync(Guid sectionId);
}
