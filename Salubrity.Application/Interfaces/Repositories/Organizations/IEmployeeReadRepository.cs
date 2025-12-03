// Salubrity.Application/Interfaces/Repositories/Organizations/IEmployeeReadRepository.cs
using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Application.Interfaces.Repositories.Organizations;

public interface IEmployeeReadRepository
{
    /// Returns UserIds for active employees in an organization (employees must have a bound user).
    Task<List<Guid>> GetActiveEmployeeUserIdsAsync(Guid organizationId, CancellationToken ct = default);
    Task<Employee?> FindByUserIdAsync(Guid userId);
}
