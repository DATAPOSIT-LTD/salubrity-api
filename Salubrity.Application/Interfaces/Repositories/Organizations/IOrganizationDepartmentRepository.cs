using Salubrity.Domain.Entities.Lookup;

namespace Salubrity.Application.Interfaces.Repositories.Organizations;

public interface IOrganizationDepartmentRepository
{
    Task<List<Department>> GetByOrganizationAsync(Guid organizationId, CancellationToken ct = default);
    Task<Department?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsByNameAsync(Guid organizationId, string name, CancellationToken ct = default);
    Task AddAsync(Department entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<Department> entities, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, CancellationToken ct = default);
}
