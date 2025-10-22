using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Infrastructure.Repositories.HealthcareServices
{
    public interface IHealthCampPackageRepository
    {
        Task<HealthCampPackage> CreateAsync(Guid campId, CreateHealthCampPackageDto dto);
        Task<List<HealthCampPackage>> GetByCampIdAsync(Guid campId);
        Task AssignPackageAsync(Guid participantId, Guid packageId);
        Task<HealthCampPackage> UpdateAsync(Guid packageId, UpdateHealthCampPackageDto dto);
        Task<List<Guid>> GetServiceIdsByCampIdAsync(Guid campId);
        Task<List<AllocatedServiceDto>> GetAllocatedServicesByCampIdAsync(Guid campId);
    }
}