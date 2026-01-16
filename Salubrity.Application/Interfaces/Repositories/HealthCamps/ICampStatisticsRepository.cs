using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface ICampStatisticsRepository
{
    Task<CampStatsResponseDto> GetCampStatsAsync(Guid campId, CancellationToken ct);
}
