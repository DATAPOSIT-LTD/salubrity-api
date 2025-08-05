// File: Salubrity.Application/Interfaces/Repositories/Forms/IFormRepository.cs
using Salubrity.Domain.Entities.Forms;
using Salubrity.Domain.Entities.FormFields;

namespace Salubrity.Application.Interfaces.Repositories.Forms;

public interface IFormRepository
{
    // Form CRUD
    Task<Form?> GetByIdAsync(Guid id);
    Task<List<Form>> GetAllAsync();
    Task<Form> CreateAsync(Form entity);
    Task<Form> UpdateAsync(Form entity);
    Task DeleteAsync(Guid id);
    Task<bool> FormExistsAsync(Guid formId);
    Task<int> GetFormCountAsync();

    // Form with related data
    Task<Form?> GetWithSectionsAsync(Guid formId); // sections + fields + options
    Task<Form?> GetWithFieldsAsync(Guid formId);   // direct fields + options
}
