// File: Salubrity.Application/Interfaces/Repositories/Forms/IFormFieldRepository.cs
using Salubrity.Domain.Entities.FormFields;

namespace Salubrity.Application.Interfaces.Repositories.Forms;

public interface IFormFieldRepository
{
    Task<FormField?> GetByIdAsync(Guid fieldId);
    Task<List<FormField>> GetByFormIdAsync(Guid formId);          // for forms without sections
    Task<List<FormField>> GetBySectionIdAsync(Guid sectionId);    // for sectioned forms
    Task<FormField> CreateAsync(FormField field);
    Task<FormField> UpdateAsync(FormField field);
    Task DeleteAsync(Guid fieldId);
}
