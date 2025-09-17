using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Services.HealthCamps;

namespace Salubrity.Application.Services.Camps;

public class MyCampQueryService : IMyCampQueryService
{
    private readonly IMyCampReadRepository _repo;

    public MyCampQueryService(IMyCampReadRepository repo)
    {
        _repo = repo;
    }

    public Task<PagedResult<MyCampListItemDto>> GetUpcomingForUserAsync(
        Guid userId, int page, int pageSize, string? search, CancellationToken ct = default)
        => _repo.GetUpcomingForUserAsync(userId, page, pageSize, search, ct);

    public Task<IReadOnlyList<MyCampServiceDto>> GetServicesForUserCampAsync(
       Guid userId,
       Guid campId,
       bool group = false,
       CancellationToken ct = default)
       => _repo.GetServicesForUserCampAsync(userId, campId, ct: ct);


}
