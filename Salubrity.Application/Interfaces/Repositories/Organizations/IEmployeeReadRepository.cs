// Salubrity.Application/Interfaces/Repositories/Organizations/IEmployeeReadRepository.cs
namespace Salubrity.Application.Interfaces.Repositories.Organizations;

public interface IEmployeeReadRepository
{
    /// Returns UserIds for active employees in an organization (employees must have a bound user).
    Task<List<Guid>> GetActiveEmployeeUserIdsAsync(Guid organizationId, CancellationToken ct = default);
}
