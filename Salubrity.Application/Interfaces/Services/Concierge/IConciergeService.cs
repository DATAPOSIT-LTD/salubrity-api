using Salubrity.Application.DTOs.Concierge;
using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.Concierge
{
    public interface IConciergeService
    {
        Task<List<CampServiceStationInfoDto>> GetCampServiceStationsAsync(Guid campId, CancellationToken ct);
        Task<List<CampQueuePriorityDto>> GetCampQueuePrioritiesAsync(Guid campId, CancellationToken ct);
        Task<List<CampServiceStationWithQueueDto>> GetCampServiceStationsWithQueueAsync(Guid campId, CancellationToken ct);
        Task<PatientDetailDto?> GetPatientDetailByIdAsync(Guid patientId, CancellationToken ct = default);
        /// <summary>
        /// Returns all service stations and statuses for a participant.
        /// </summary>
        Task<List<ParticipantStationStatusDto>> GetParticipantStationsAsync(Guid participantId, CancellationToken ct = default);

    }
}
