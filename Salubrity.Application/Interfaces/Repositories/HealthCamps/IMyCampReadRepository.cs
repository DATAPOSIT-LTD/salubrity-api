using Salubrity.Application.DTOs.HealthAssessment;
using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.Camps;

public interface IMyCampReadRepository
{
    Task<PagedResult<MyCampListItemDto>> GetUpcomingForUserAsync(
        Guid userId, int page, int pageSize, string? search, CancellationToken ct = default);

    Task<IReadOnlyList<MyCampServiceDto>> GetServicesForUserCampAsync(
       Guid userId,
       Guid campId,
       bool group = false,
       CancellationToken ct = default);
}
