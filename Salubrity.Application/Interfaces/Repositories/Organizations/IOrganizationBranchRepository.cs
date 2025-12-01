using Salubrity.Domain.Entities.Organizations;

namespace Salubrity.Application.Interfaces.Repositories.Organizations;

public interface IOrganizationBranchRepository
{
    Task<OrganizationBranch> CreateAsync(OrganizationBranch branch);
    Task<List<OrganizationBranch>> GetByOrganizationAsync(Guid organizationId);
    Task<OrganizationBranch?> GetByIdAsync(Guid id);
    Task UpdateAsync(OrganizationBranch branch);
    Task DeleteAsync(Guid id);
}
