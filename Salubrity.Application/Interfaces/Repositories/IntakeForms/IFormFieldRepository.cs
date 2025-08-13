// File: Salubrity.Application/Interfaces/Repositories/Forms/IFormFieldRepository.cs
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Interfaces.Repositories.IntakeForms;

public interface IFormFieldRepository
{
    Task<IntakeFormField?> GetByIdAsync(Guid fieldId);
    Task<List<IntakeFormField>> GetByFormIdAsync(Guid formId);          // for forms without sections
    Task<List<IntakeFormField>> GetBySectionIdAsync(Guid sectionId);    // for sectioned forms
    Task<IntakeFormField> CreateAsync(IntakeFormField field);
    Task<IntakeFormField> UpdateAsync(IntakeFormField field);
    Task DeleteAsync(Guid fieldId);
}
