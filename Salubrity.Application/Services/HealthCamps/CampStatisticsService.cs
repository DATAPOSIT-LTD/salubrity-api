using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;

namespace Salubrity.Application.Services.HealthCamps;

public class CampStatisticsService : ICampStatisticsService
{
    private readonly ICampStatisticsRepository _repo;

    public CampStatisticsService(ICampStatisticsRepository repo)
    {
        _repo = repo;
    }

    public Task<CampStatsResponseDto> GetCampStatsAsync(Guid campId, CancellationToken ct)
        => _repo.GetCampStatsAsync(campId, ct);
}
