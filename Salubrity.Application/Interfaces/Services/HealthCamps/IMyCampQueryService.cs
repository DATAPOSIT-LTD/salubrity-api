using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.HealthCamps;

public interface IMyCampQueryService
{
    Task<PagedResult<MyCampListItemDto>> GetUpcomingForUserAsync(
        Guid userId, int page, int pageSize, string? search, CancellationToken ct = default);

    Task<PagedResult<MyCampListItemDto>> GetOngoingForUserAsync(
        Guid userId, int page, int pageSize, string? search, CancellationToken ct = default);

    Task<PagedResult<MyCampListItemDto>> GetCompletedForUserAsync(
        Guid userId, int page, int pageSize, string? search, CancellationToken ct = default);

    Task<IReadOnlyList<MyCampServiceDto>> GetServicesForUserCampAsync(
        Guid userId,
        Guid campId,
        bool group = false,
        CancellationToken ct = default);




}
