using System;
using System.Threading;
using System.Threading.Tasks;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps
{
    public interface IHealthCampPackageRepository
    {
        Task<HealthCampPackage?> GetPackageByCampAsync(Guid campId, Guid packageId, CancellationToken ct = default);
        Task<IReadOnlyList<HealthCampPackage>> GetAllPackagesWithServicesByCampAsync(Guid campId, CancellationToken ct = default);
    }
}
