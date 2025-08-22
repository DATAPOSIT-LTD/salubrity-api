using Salubrity.Application.DTOs.HomepageOverview;

namespace Salubrity.Application.Interfaces.Services.HomepageOverview
{
    public interface IHomepageOverviewService
    {
        Task<HomepageOverviewDto> GetHomepageOverviewAsync();
    }
}
