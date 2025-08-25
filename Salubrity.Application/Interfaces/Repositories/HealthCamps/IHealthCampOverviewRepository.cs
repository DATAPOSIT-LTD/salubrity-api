using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps
{
    public interface IHealthCampOverviewRepository
    {
        Task<HealthCampOverviewDto> GetHealthCampOverviewAsync();
    }
}
