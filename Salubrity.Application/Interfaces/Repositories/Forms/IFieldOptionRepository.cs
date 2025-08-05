// File: Salubrity.Application/Interfaces/Repositories/Forms/IFieldOptionRepository.cs
using Salubrity.Domain.Entities.FormsOptions;

namespace Salubrity.Application.Interfaces.Repositories.Forms;

public interface IFieldOptionRepository
{
    Task<List<FieldOption>> GetByFieldIdAsync(Guid fieldId);
    Task<FieldOption> CreateAsync(FieldOption option);
    Task DeleteAsync(Guid optionId);
}
