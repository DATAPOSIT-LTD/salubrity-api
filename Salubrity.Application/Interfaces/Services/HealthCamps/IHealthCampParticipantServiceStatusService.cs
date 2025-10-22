// File: Salubrity.Application.Interfaces.Services.HealthCamps.IHealthCampParticipantServiceStatusService.cs

using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.HealthCamps;

public interface IHealthCampParticipantServiceStatusService
{
    Task MarkServedAsync(Guid participantId, Guid serviceAssignmentId, Guid? subcontractorId, CancellationToken ct);
    Task<List<HealthCampParticipantServiceStatus>> GetStatusesByParticipantAsync(Guid participantId, CancellationToken ct);
}
