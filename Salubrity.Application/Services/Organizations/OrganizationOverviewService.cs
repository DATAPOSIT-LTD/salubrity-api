using Salubrity.Application.DTOs.Organizations;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Services.Organizations;

namespace Salubrity.Application.Services.Organizations
{
    public class OrganizationOverviewService : IOrganizationOverviewService
    {
        private readonly IOrganizationOverviewRepository _repo;

        public OrganizationOverviewService(IOrganizationOverviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<OrganizationOverviewDto> GetOrganizationOverviewAsync()
        {
            return await _repo.GetOrganizationOverviewAsync();
        }

    }
}