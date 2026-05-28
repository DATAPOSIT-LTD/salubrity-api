using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.HealthCamps;

public interface ICampStatisticsService
{
    Task<CampStatsResponseDto> GetCampStatsAsync(Guid campId, CancellationToken ct);
    Task<CampSaStatusDto> GetCampSaStatusAsync(Guid campId, CancellationToken ct);
}
