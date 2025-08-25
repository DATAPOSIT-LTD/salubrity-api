using System;
using System.Threading;
using System.Threading.Tasks;

namespace Salubrity.Application.Common.Interfaces.Repositories
{
    public interface IHealthCampParticipantRepository
    {
        Task<Guid?> GetPatientIdByParticipantIdAsync(Guid participantId, CancellationToken ct = default);
    }
}
