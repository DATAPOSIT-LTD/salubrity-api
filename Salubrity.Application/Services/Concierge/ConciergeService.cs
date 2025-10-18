using Salubrity.Application.DTOs.Concierge;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Concierge;
using Salubrity.Application.Interfaces.Services.Concierge;
using Salubrity.Application.Interfaces.Services.HealthCamps;

namespace Salubrity.Application.Services.Concierge
{
    public class ConciergeService : IConciergeService
    {
        private readonly IConciergeRepository _repo;
        private readonly IHealthCampCheckInService _checkInService;

        public ConciergeService(
            IConciergeRepository repo,
            IHealthCampCheckInService checkInService)
        {
            _repo = repo;
            _checkInService = checkInService;
        }

        public Task<List<CampServiceStationInfoDto>> GetCampServiceStationsAsync(Guid campId, CancellationToken ct)
            => _repo.GetCampServiceStationsAsync(campId, ct);

        public Task<List<CampQueuePriorityDto>> GetCampQueuePrioritiesAsync(Guid campId, CancellationToken ct)
            => _repo.GetCampQueuePrioritiesAsync(campId, ct);

        public Task<List<CampServiceStationWithQueueDto>> GetCampServiceStationsWithQueueAsync(Guid campId, CancellationToken ct)
            => _repo.GetCampServiceStationsWithQueueAsync(campId, ct);

        public Task<PatientDetailDto?> GetPatientDetailByIdAsync(Guid patientId, CancellationToken ct = default)
            => _repo.GetPatientDetailByIdAsync(patientId, ct);

        /// <summary>
        /// Returns all service stations and statuses for a participant.
        /// </summary>
        public async Task<List<ParticipantStationStatusDto>> GetParticipantStationsAsync(Guid participantId, CancellationToken ct = default)
        {
            return await _checkInService.GetParticipantStationStatusesAsync(participantId, ct);
        }
    }
}
