using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Join;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampCheckInRepository
{
    Task<HealthCampParticipant?> GetParticipantAsync(Guid participantId, CancellationToken ct = default);
    Task<List<(Guid Id, string? AssignmentName)>> GetAssignmentsAsync(Guid healthCampId, CancellationToken ct = default);
    Task<List<HealthCampStationCheckIn>> GetCheckInsAsync(Guid participantId, CancellationToken ct = default);
}
