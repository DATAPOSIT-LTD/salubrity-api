using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Interfaces.Repositories.IntakeForms;

public interface IIntakeFormRepository
{
    Task<IntakeForm?> GetByIdAsync(Guid id);
    Task<List<IntakeForm>> GetAllAsync();
    Task<IntakeForm> CreateAsync(IntakeForm entity);
    Task<IntakeForm> UpdateAsync(IntakeForm entity);
    Task DeleteAsync(Guid id);
    Task<bool> FormExistsAsync(Guid formId);
    Task<int> GetFormCountAsync();

    // Form with related data
    Task<IntakeForm?> GetWithSectionsAsync(Guid formId); // sections + fields + options
    Task<IntakeForm?> GetWithFieldsAsync(Guid formId);   // direct fields + options
    /// <summary>
    /// Checks if an intake form is assigned to any service, category, or subcategory
    /// </summary>
    Task<bool> IsFormAssignedAnywhereAsync(Guid formId);
    // IIntakeFormRepository.cs
    Task<List<IntakeForm>> GetLabFormsByIdsAsync(HashSet<Guid> ids, CancellationToken ct);

}
