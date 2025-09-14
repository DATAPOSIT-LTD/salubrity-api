// File: Salubrity.Application/Interfaces/Repositories/Camps/ICampQueueRepository.cs
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.Camps;

public interface ICampQueueRepository
{
    Task CheckInAsync(Guid userId, CheckInRequestDto dto, CancellationToken ct = default);
    Task CancelMyCheckInAsync(Guid userId, Guid campId, CancellationToken ct = default);
    Task<CheckInStateDto> GetMyCheckInStateAsync(Guid userId, Guid campId, CancellationToken ct = default);
    Task<QueuePositionDto> GetMyPositionAsync(Guid userId, Guid campId, Guid assignmentId, CancellationToken ct = default);
    Task StartServiceAsync(Guid staffUserId, Guid checkInId, CancellationToken ct = default);
    Task CompleteServiceAsync(Guid staffUserId, Guid checkInId, CancellationToken ct = default);
    Task<HealthCampStationCheckIn?> GetActiveForParticipantAsync(
               Guid participantId,
               Guid? assignmentId,
               CancellationToken ct = default);

    Task<HealthCampStationCheckIn?> GetByIdAsync(Guid checkInId, CancellationToken ct = default);

    Task UpdateAsync(HealthCampStationCheckIn checkIn, CancellationToken ct = default);

    Task<HealthCampStationCheckIn?> GetLatestForParticipantAsync(
        Guid participantId, CancellationToken ct = default)
}
