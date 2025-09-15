using Salubrity.Application.DTOs.Concierge;
using Salubrity.Application.Interfaces.Repositories.Concierge;
using Salubrity.Application.Interfaces.Services.Concierge;

namespace Salubrity.Application.Services.Concierge
{
    public class ConciergeService : IConciergeService
    {
        private readonly IConciergeRepository _repo;
        public ConciergeService(IConciergeRepository repo) => _repo = repo;

        public Task<List<CampServiceStationInfoDto>> GetCampServiceStationsAsync(Guid campId, CancellationToken ct)
        => _repo.GetCampServiceStationsAsync(campId, ct);

        public Task<List<CampQueuePriorityDto>> GetCampQueuePrioritiesAsync(Guid campId, CancellationToken ct)
            => _repo.GetCampQueuePrioritiesAsync(campId, ct);
    }
}
