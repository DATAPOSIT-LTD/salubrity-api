using Salubrity.Application.DTOs.HomepageOverview;

namespace Salubrity.Application.Interfaces.Repositories.HomepageOverview
{
    public interface IHomepageOverviewRepository
    {
        Task<HomepageOverviewDto> GetHomepageOverviewAsync();
    }
}
