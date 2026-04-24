// File: Application/Interfaces/Repositories/Clinical/IServiceReferralRepository.cs
using Salubrity.Domain.Entities.Clinical;

namespace Salubrity.Application.Interfaces.Repositories.Clinical
{
    public interface IServiceReferralRepository
    {
        Task<ServiceReferral?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<ServiceReferral>> GetByParticipantAndCampAsync(Guid participantId, Guid healthCampId, CancellationToken ct = default);
        Task<IReadOnlyList<ServiceReferral>> GetByCampAsync(Guid healthCampId, CancellationToken ct = default);
        Task<ServiceReferral> CreateAsync(ServiceReferral entity, CancellationToken ct = default);
        Task<ServiceReferral> UpdateAsync(ServiceReferral entity, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);

        // Resolves the parent HealthCampId of a service assignment so the service layer
        // does not need to trust client-supplied campId.
        Task<Guid?> GetHealthCampIdForAssignmentAsync(Guid serviceAssignmentId, CancellationToken ct = default);
    }
}
