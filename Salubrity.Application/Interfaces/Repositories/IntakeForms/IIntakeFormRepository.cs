using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Interfaces.Repositories.IntakeForms;

public interface IIntakeFormRepository
{
    Task<List<IntakeForm>> GetAllAsync();
    Task<IntakeForm?> GetByIdAsync(Guid id);
    Task AddAsync(IntakeForm form);
    Task UpdateAsync(IntakeForm form);
    Task DeleteAsync(IntakeForm form);
}
