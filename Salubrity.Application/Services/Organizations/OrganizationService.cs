using AutoMapper;
using Salubrity.Application.DTOs.Organizations;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Services.Organizations;
using Salubrity.Domain.Entities.Organizations;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Organizations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repository;
        private readonly IMapper _mapper;

        public OrganizationService(IOrganizationRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<OrganizationResponseDto> CreateAsync(OrganizationCreateDto dto)
        {
            var org = _mapper.Map<Organization>(dto);
            var created = await _repository.CreateAsync(org);
            return _mapper.Map<OrganizationResponseDto>(created);
        }

        public async Task<OrganizationResponseDto> GetByIdAsync(Guid id)
        {
            var org = await _repository.GetByIdAsync(id);
            if (org == null)
                throw new NotFoundException("Organization", id.ToString());

            return _mapper.Map<OrganizationResponseDto>(org);
        }

        public async Task<List<OrganizationResponseDto>> GetAllAsync()
        {
            var list = await _repository.GetAllAsync();
            return _mapper.Map<List<OrganizationResponseDto>>(list);
        }

        public async Task UpdateAsync(Guid id, OrganizationUpdateDto dto)
        {
            var org = await _repository.GetByIdAsync(id);
            if (org == null)
                throw new NotFoundException("Organization", id.ToString());

            _mapper.Map(dto, org);
            await _repository.UpdateAsync(org);
        }

        public async Task DeleteAsync(Guid id)
        {
            var org = await _repository.GetByIdAsync(id);
            if (org == null)
                throw new NotFoundException("Organization", id.ToString());

            await _repository.DeleteAsync(id);
        }
    }
}
