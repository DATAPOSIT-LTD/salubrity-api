using Salubrity.Domain.Entities.Join;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Salubrity.Application.Common.Interfaces.Repositories
{
    public interface IHealthCampParticipantRepository
    {
        Task<Guid?> GetPatientIdByParticipantIdAsync(Guid participantId, CancellationToken ct = default);
        Task<bool> IsParticipantLinkedToCampAsync(Guid campId, Guid userId, CancellationToken ct = default);
        Task AddParticipantAsync(Domain.Entities.Join.HealthCampParticipant participant, CancellationToken ct = default);
        Task<HealthCampParticipant?> GetParticipantAsync(Guid campId, Guid participantId, CancellationToken ct = default);
        Task UpdateParticipantAsync(HealthCampParticipant participant, CancellationToken ct = default);
    }
}
