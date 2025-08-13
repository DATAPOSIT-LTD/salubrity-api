// File: Salubrity.Application/Interfaces/Repositories/Forms/IFormBuilderRepository.cs
#nullable enable

using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Interfaces.Repositories.IntakeForms
{
    public interface IFormBuilderRepository
    {
        Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
        Task AddFormGraphAsync(IntakeForm form, CancellationToken ct = default);
        Task<IntakeForm?> LoadFormWithFieldsAsync(Guid formId, CancellationToken ct = default);
    }
}
