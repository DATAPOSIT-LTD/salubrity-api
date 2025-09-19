// File: Salubrity.Application/Services/Camps/CampQueueService.cs
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Services.Camps;

namespace Salubrity.Application.Services.Camps;

public class CampQueueService : ICampQueueService
{
    private readonly ICampQueueRepository _repo;
    public CampQueueService(ICampQueueRepository repo) => _repo = repo;

    public Task CheckInAsync(Guid userId, CheckInRequestDto dto, CancellationToken ct = default)
        => _repo.CheckInAsync(userId, dto, ct);

    public Task CancelMyCheckInAsync(Guid userId, Guid campId, CancellationToken ct = default)
        => _repo.CancelMyCheckInAsync(userId, campId, ct);

    public Task<CheckInStateDto> GetMyCheckInStateAsync(Guid userId, Guid campId, CancellationToken ct = default)
        => _repo.GetMyCheckInStateAsync(userId, campId, ct);

    public Task<QueuePositionDto> GetMyPositionAsync(Guid userId, Guid campId, Guid assignmentId, CancellationToken ct = default)
        => _repo.GetMyPositionAsync(userId, campId, assignmentId, ct);

    public Task StartServiceAsync(Guid staffUserId, Guid checkInId, CancellationToken ct = default)
        => _repo.StartServiceAsync(staffUserId, checkInId, ct);

    public Task CompleteServiceAsync(Guid staffUserId, Guid checkInId, CancellationToken ct = default)
        => _repo.CompleteServiceAsync(staffUserId, checkInId, ct);

    public async Task<List<MyQueuedParticipantDto>> GetMyQueueAsync(Guid userId, Guid campId, Guid? subcontractorId, CancellationToken ct = default)
    {
        return await _repo.GetQueuedParticipantsAsync(userId, campId, subcontractorId, ct);
    }
}
