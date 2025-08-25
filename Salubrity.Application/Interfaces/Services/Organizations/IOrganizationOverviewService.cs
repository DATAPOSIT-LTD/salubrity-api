using Salubrity.Application.DTOs.Organizations;

namespace Salubrity.Application.Interfaces.Services.Organizations
{
    public interface IOrganizationOverviewService
    {
        Task<OrganizationOverviewDto> GetOrganizationOverviewAsync();
    }
}
