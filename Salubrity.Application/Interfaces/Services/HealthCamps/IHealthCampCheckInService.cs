using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.HealthCamps;

public interface IHealthCampCheckInService
{
    /// <summary>
    /// Returns all service station check-in statuses for a given participant in a health camp.
    /// </summary>
    Task<List<ParticipantStationStatusDto>> GetParticipantStationStatusesAsync(Guid participantId, CancellationToken ct = default);
}
