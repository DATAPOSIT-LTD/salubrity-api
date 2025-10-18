using AutoMapper;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Infrastructure.Repositories.HealthcareServices;

namespace Salubrity.Application.Services.HealthcareServices
{
    public class HealthCampPackageService : IHealthCampPackageService
    {
        private readonly IHealthCampPackageRepository _repo;
        private readonly IMapper _mapper;

        public HealthCampPackageService(IHealthCampPackageRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<HealthCampPackageDto>> CreatePackagesAsync(Guid campId, CreateHealthCampPackagesDto dto)
        {
            var result = new List<HealthCampPackageDto>();
            foreach (var pkg in dto.Packages)
            {
                var entity = await _repo.CreateAsync(campId, pkg);
                result.Add(_mapper.Map<HealthCampPackageDto>(entity));
            }
            return result;
        }

        public async Task<List<HealthCampPackageDto>> GetPackagesAsync(Guid campId)
        {
            var entities = await _repo.GetByCampIdAsync(campId);
            return _mapper.Map<List<HealthCampPackageDto>>(entities);
        }

        public async Task AssignPackageAsync(PickPackageDto dto)
        {
            await _repo.AssignPackageAsync(dto.ParticipantId, dto.HealthCampPackageId);
        }

        public async Task<HealthCampPackageDto> UpdatePackageAsync(Guid packageId, UpdateHealthCampPackageDto dto)
        {
            var entity = await _repo.UpdateAsync(packageId, dto);
            return _mapper.Map<HealthCampPackageDto>(entity);
        }

        public async Task<List<Guid>> GetAllocatedServiceIdsAsync(Guid campId)
        {
            return await _repo.GetServiceIdsByCampIdAsync(campId);
        }

        public async Task<List<AllocatedServiceDto>> GetAllocatedServicesAsync(Guid campId)
        {
            return await _repo.GetAllocatedServicesByCampIdAsync(campId);
        }
    }
}