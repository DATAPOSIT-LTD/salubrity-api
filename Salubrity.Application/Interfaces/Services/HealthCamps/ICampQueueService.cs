// File: Salubrity.Application/Interfaces/Services/Camps/ICampQueueService.cs
using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.Camps;

public interface ICampQueueService
{
    Task CheckInAsync(Guid userId, CheckInRequestDto dto, CancellationToken ct = default);
    Task CancelMyCheckInAsync(Guid userId, Guid campId, CancellationToken ct = default);
    Task<CheckInStateDto> GetMyCheckInStateAsync(Guid userId, Guid campId, CancellationToken ct = default);
    Task<QueuePositionDto> GetMyPositionAsync(Guid userId, Guid campId, Guid assignmentId, CancellationToken ct = default);
    Task StartServiceAsync(Guid staffUserId, Guid checkInId, CancellationToken ct = default);
    Task CompleteServiceAsync(Guid staffUserId, Guid checkInId, CancellationToken ct = default);
    Task<List<QueuedParticipantDto>> GetMyQueueAsync(Guid userId, Guid campId, Guid? subcontractorId, CancellationToken ct = default);

}
