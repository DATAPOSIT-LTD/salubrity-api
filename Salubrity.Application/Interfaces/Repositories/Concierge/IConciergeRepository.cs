using Salubrity.Application.DTOs.Concierge;

namespace Salubrity.Application.Interfaces.Repositories.Concierge
{
    public interface IConciergeRepository
    {
        Task<List<CampServiceStationInfoDto>> GetCampServiceStationsAsync(Guid campId, CancellationToken ct);
        Task<List<CampQueuePriorityDto>> GetCampQueuePrioritiesAsync(Guid campId, CancellationToken ct);
        Task<List<CampServiceStationWithQueueDto>> GetCampServiceStationsWithQueueAsync(Guid campId, CancellationToken ct);
    }
}
