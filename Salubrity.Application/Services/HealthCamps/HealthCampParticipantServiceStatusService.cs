// File: Salubrity.Application.Services.HealthCamps.HealthCampParticipantServiceStatusService.cs

using Microsoft.Extensions.Logging;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Services.HealthCamps;

public class HealthCampParticipantServiceStatusService : IHealthCampParticipantServiceStatusService
{
    private readonly IHealthCampParticipantServiceStatusRepository _repo;
    private readonly ILogger<HealthCampParticipantServiceStatusService> _logger;

    public HealthCampParticipantServiceStatusService(
        IHealthCampParticipantServiceStatusRepository repo,
        ILogger<HealthCampParticipantServiceStatusService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task MarkServedAsync(Guid participantId, Guid serviceAssignmentId, Guid? subcontractorId, CancellationToken ct)
    {
        var existing = await _repo.GetByParticipantAndAssignmentAsync(participantId, serviceAssignmentId, ct);

        if (existing == null)
        {
            var newRecord = new HealthCampParticipantServiceStatus
            {
                Id = Guid.NewGuid(),
                ParticipantId = participantId,
                ServiceAssignmentId = serviceAssignmentId,
                SubcontractorId = subcontractorId,
                ServedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(newRecord, ct);
            _logger.LogInformation("✅ Added service status for participant {ParticipantId} at assignment {AssignmentId}", participantId, serviceAssignmentId);
        }
        else if (existing.ServedAt == null)
        {
            existing.ServedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(existing, ct);
            _logger.LogInformation("🔁 Updated service status timestamp for participant {ParticipantId}", participantId);
        }
    }

    public async Task<List<HealthCampParticipantServiceStatus>> GetStatusesByParticipantAsync(Guid participantId, CancellationToken ct)
    {
        return await _repo.GetByParticipantAsync(participantId, ct);
    }
}
