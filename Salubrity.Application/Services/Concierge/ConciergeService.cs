using Salubrity.Application.DTOs.Concierge;
using Salubrity.Application.Interfaces.Repositories.Concierge;
using Salubrity.Application.Interfaces.Services.Concierge;

namespace Salubrity.Application.Services.Concierge
{
    public class ConciergeService : IConciergeService
    {
        private readonly IConciergeRepository _repo;
        public ConciergeService(IConciergeRepository repo) => _repo = repo;

        // Get all service stations queue info for a camp
        public Task<List<CampServiceStationInfoDto>> GetCampServiceStationsAsync(Guid campId, CancellationToken ct)
        => _repo.GetCampServiceStationsAsync(campId, ct);
    }
}
