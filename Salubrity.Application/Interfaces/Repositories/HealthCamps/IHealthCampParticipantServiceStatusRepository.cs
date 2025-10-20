// File: Salubrity.Application.Interfaces.Repositories.HealthCamps.IHealthCampParticipantServiceStatusRepository.cs

using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampParticipantServiceStatusRepository
{
    Task<HealthCampParticipantServiceStatus?> GetByParticipantAndAssignmentAsync(Guid participantId, Guid serviceAssignmentId, CancellationToken ct);
    Task AddAsync(HealthCampParticipantServiceStatus entity, CancellationToken ct);
    Task UpdateAsync(HealthCampParticipantServiceStatus entity, CancellationToken ct);
    Task<List<HealthCampParticipantServiceStatus>> GetByParticipantAsync(Guid participantId, CancellationToken ct);
    Task<List<HealthCampParticipantServiceStatus>> GetByCampAsync(Guid campId, CancellationToken ct);
}
