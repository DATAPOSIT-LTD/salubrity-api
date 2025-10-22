using Salubrity.Application.DTOs.HealthcareServices;

namespace Salubrity.Application.Interfaces.Services.HealthcareServices
{
    public interface IHealthCampPackageService
    {
        Task<List<HealthCampPackageDto>> CreatePackagesAsync(Guid campId, CreateHealthCampPackagesDto dto);
        Task<List<HealthCampPackageDto>> GetPackagesAsync(Guid campId);
        Task AssignPackageAsync(PickPackageDto dto);
        Task<HealthCampPackageDto> UpdatePackageAsync(Guid packageId, UpdateHealthCampPackageDto dto);
        Task<List<Guid>> GetAllocatedServiceIdsAsync(Guid campId);
        Task<List<AllocatedServiceDto>> GetAllocatedServicesAsync(Guid campId);
    }
}
