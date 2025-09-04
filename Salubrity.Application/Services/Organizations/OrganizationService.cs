using AutoMapper;
using Salubrity.Application.DTOs.Organizations;
using Salubrity.Application.Interfaces.Repositories.Organizations;
using Salubrity.Application.Interfaces.Services.Notifications;
using Salubrity.Application.Interfaces.Services.Organizations;
using Salubrity.Domain.Entities.Organizations;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Organizations
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IOrganizationRepository _repository;
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;

        public OrganizationService(IOrganizationRepository repository, INotificationService notificationService, IMapper mapper)
        {
            _repository = repository;
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<OrganizationResponseDto> CreateAsync(OrganizationCreateDto dto)
        {
            var ct = CancellationToken.None;
            var org = _mapper.Map<Organization>(dto);
            var created = await _repository.CreateAsync(org);
            await _notificationService.TriggerNotificationAsync(
                title: "New Organization Created",
                message: $"Organization '{org.BusinessName}' has been created.",
                type: "Organization",
                entityId: org.Id,
                entityType: "Organization",
                ct: ct
            );

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
            var ct = CancellationToken.None;
            var org = await _repository.GetByIdAsync(id);
            if (org == null)
                throw new NotFoundException("Organization", id.ToString());

            _mapper.Map(dto, org);
            await _repository.UpdateAsync(org);

            await _notificationService.TriggerNotificationAsync(
                title: "Organization Updated",
                message: $"Organization '{org.BusinessName}' has been updated.",
                type: "Organization",
                entityId: org.Id,
                entityType: "Organization",
                ct: ct
            );
        }

        public async Task DeleteAsync(Guid id)
        {
            var ct = CancellationToken.None;
            var org = await _repository.GetByIdAsync(id);
            if (org == null)
                throw new NotFoundException("Organization", id.ToString());

            await _repository.DeleteAsync(id);

            await _notificationService.TriggerNotificationAsync(
                title: "Organization Deleted",
                message: $"Organization with ID '{id}' has been deleted.",
                type: "Organization",
                entityId: id,
                entityType: "Organization",
                ct: ct
            );
        }

        public async Task<OrganizationResponseDto> GetByNameAsync(string name)
        {
            var org = await _repository.FindByNameAsync(name);
            if (org == null)
                throw new NotFoundException("Organization", name);

            return _mapper.Map<OrganizationResponseDto>(org);
        }
    }
}
