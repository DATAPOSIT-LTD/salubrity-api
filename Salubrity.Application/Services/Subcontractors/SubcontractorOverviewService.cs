using Salubrity.Application.DTOs.Subcontractor;
using Salubrity.Application.Interfaces.Repositories.Subcontactors;
using Salubrity.Application.Interfaces.Services.Subcontractors;

namespace Salubrity.Application.Services.Subcontractors
{
    public class SubcontractorOverviewService : ISubcontractorOverviewService
    {
        private readonly ISubcontractorOverviewRepository _repo;

        public SubcontractorOverviewService(ISubcontractorOverviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<SubcontractorOverviewDto> GetSubcontractorOverviewAsync()
        {
            return await _repo.GetSubcontractorOverviewAsync();
        }
    }
}
