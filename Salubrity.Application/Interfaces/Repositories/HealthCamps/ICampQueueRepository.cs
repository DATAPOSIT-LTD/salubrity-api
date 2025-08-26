// File: Salubrity.Application/Interfaces/Repositories/Camps/ICampQueueRepository.cs
using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.Camps;

public interface ICampQueueRepository
{
    Task CheckInAsync(Guid userId, CheckInRequestDto dto, CancellationToken ct = default);
    Task CancelMyCheckInAsync(Guid userId, Guid campId, CancellationToken ct = default);
    Task<CheckInStateDto> GetMyCheckInStateAsync(Guid userId, Guid campId, CancellationToken ct = default);
    Task<QueuePositionDto> GetMyPositionAsync(Guid userId, Guid campId, Guid assignmentId, CancellationToken ct = default);
    Task StartServiceAsync(Guid staffUserId, Guid checkInId, CancellationToken ct = default);
    Task CompleteServiceAsync(Guid staffUserId, Guid checkInId, CancellationToken ct = default);
}
