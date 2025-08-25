using Salubrity.Application.DTOs.Organizations;

namespace Salubrity.Application.Interfaces.Repositories.Organizations
{
    public interface IOrganizationOverviewRepository
    {
        Task<OrganizationOverviewDto> GetOrganizationOverviewAsync();
    }
}
