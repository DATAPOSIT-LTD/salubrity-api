// File: Salubrity.Application/Interfaces/Repositories/Forms/IFieldOptionRepository.cs

using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Interfaces.Repositories.IntakeForms;

public interface IFieldOptionRepository
{
    Task<List<IntakeFormFieldOption>> GetByFieldIdAsync(Guid fieldId);
    Task<IntakeFormFieldOption> CreateAsync(IntakeFormFieldOption option);
    Task DeleteAsync(Guid optionId);
}
