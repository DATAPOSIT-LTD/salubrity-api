using Salubrity.Application.DTOs.Concierge;

namespace Salubrity.Application.Interfaces.Services.Concierge
{
    public interface IConciergeService
    {
        Task<List<CampServiceStationInfoDto>> GetCampServiceStationsAsync(Guid campId, CancellationToken ct);
    }
}
