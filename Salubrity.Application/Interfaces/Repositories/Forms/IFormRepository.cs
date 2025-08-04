using Salubrity.Domain.Entities.FormFields;
using Salubrity.Domain.Entities.Forms;

namespace Salubrity.Application.Interfaces.Repositories.Forms;

public interface IFormRepository
{
    Task<Form> GetByIdAsync(Guid id);
    Task<List<Form>> GetAllAsync();
    Task<Form> CreateAsync(Form entity);
    Task<Form> UpdateAsync(Form entity) ;
    Task DeleteAsync(Guid id);

    // Form-specific operations
    Task<Form> GetFormWithSectionsAndFieldsAsync(Guid formId);
    Task<Form> GetFormWithFieldsAsync(Guid id);
    Task<Form> GetFormWithFieldsAndOptionsAsync(Guid id);

    // Field-specific operations
    Task<FormField> GetFormFieldWithOptionsAsync(Guid fieldId);
    Task<bool> FieldExistsInFormAsync(Guid formId, Guid fieldId);
    // Bulk operations
    Task AddFormFieldsAsync(Guid formId, IEnumerable<FormField> fields);
    Task UpdateFormFieldsAsync(Guid formId, IEnumerable<FormField> fields);
    Task RemoveFieldsAsync(Guid formId, Guid fieldId);

    // Utility methods
    Task<bool> FormExistsAsync(Guid formId);
    Task<int> GetFormCountAsync();

}