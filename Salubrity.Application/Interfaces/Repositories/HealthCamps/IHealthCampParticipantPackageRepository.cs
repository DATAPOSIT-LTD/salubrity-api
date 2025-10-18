using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps
{
    public interface IHealthCampParticipantPackageRepository
    {
        Task<HealthCampParticipantPackage?> GetByParticipantIdAsync(Guid participantId, CancellationToken ct = default);
        Task AddAsync(HealthCampParticipantPackage entity, CancellationToken ct = default);
        Task UpdateAsync(HealthCampParticipantPackage entity, CancellationToken ct = default);
    }
}
