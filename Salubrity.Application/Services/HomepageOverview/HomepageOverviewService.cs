using Salubrity.Application.DTOs.HomepageOverview;
using Salubrity.Application.Interfaces.Repositories.HomepageOverview;
using Salubrity.Application.Interfaces.Services.HomepageOverview;

namespace Salubrity.Application.Services.HomepageOverview
{
    public class HomepageOverviewService : IHomepageOverviewService
    {
        private readonly IHomepageOverviewRepository _repo;

        public HomepageOverviewService(IHomepageOverviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<HomepageOverviewDto> GetHomepageOverviewAsync()
        {
            return await _repo.GetHomepageOverviewAsync();
        }
    }
}