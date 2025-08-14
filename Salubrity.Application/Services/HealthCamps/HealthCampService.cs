using AutoMapper;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.Lookups;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Exceptions;
using System.Text.Json.Serialization;

namespace Salubrity.Application.Services.HealthCamps;

public class HealthCampService : IHealthCampService
{
    private readonly IHealthCampRepository _repo;
    private readonly ILookupRepository<HealthCampStatus> _lookupRepository;
    private readonly IMapper _mapper;
    private readonly IPackageReferenceResolver _referenceResolver;

    public HealthCampService(IHealthCampRepository repo, ILookupRepository<HealthCampStatus> lookupRepository, IPackageReferenceResolver _pResolver, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
        _referenceResolver = _pResolver;
        _lookupRepository = lookupRepository;
    }

    public async Task<List<HealthCampListDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<List<HealthCampListDto>>(items);
    }

    public async Task<HealthCampDetailDto> GetByIdAsync(Guid id)
    {
        var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");
        return _mapper.Map<HealthCampDetailDto>(camp);
    }

    public async Task<HealthCampDto> CreateAsync(CreateHealthCampDto dto)
    {
        var upcomingStatus = await _lookupRepository.FindByNameAsync("Upcoming");

        if (upcomingStatus == null || upcomingStatus.Id == Guid.Empty)
            throw new InvalidOperationException("Upcoming status not found");

        var entity = new HealthCamp
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            ServicePackageId = dto.ServicePackageId,
            Description = dto.Description,
            Location = dto.Location,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            StartTime = dto.StartTime,
            OrganizationId = dto.OrganizationId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            PackageItems = [],
            ExpectedParticipants = dto.ExpectedParticipants,
            HealthCampStatusId = upcomingStatus.Id,
            ServiceAssignments = []
        };


        foreach (var item in dto.PackageItems)
        {
            var referenceType = await _referenceResolver.ResolveTypeAsync(item.ReferenceId);

            entity.PackageItems.Add(new HealthCampPackageItem
            {
                Id = Guid.NewGuid(),
                HealthCampId = entity.Id,
                ReferenceId = item.ReferenceId,
                ReferenceType = referenceType
            });
        }

        foreach (var assignment in dto.ServiceAssignments)
        {
            entity.ServiceAssignments.Add(new HealthCampServiceAssignment
            {
                Id = Guid.NewGuid(),
                HealthCampId = entity.Id,
                ServiceId = assignment.ServiceId,
                SubcontractorId = assignment.SubcontractorId
            });
        }

        var created = await _repo.CreateAsync(entity);
        return _mapper.Map<HealthCampDto>(created);
    }



    public async Task<HealthCampDto> UpdateAsync(Guid id, UpdateHealthCampDto dto)
    {
        var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");

        camp.Name = dto.Name;
        camp.Description = dto.Description;
        camp.Location = dto.Location;
        camp.StartDate = dto.StartDate;
        camp.EndDate = dto.EndDate;
        camp.IsActive = dto.IsActive;
        camp.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateAsync(camp);
        return _mapper.Map<HealthCampDto>(camp);
    }

    public async Task DeleteAsync(Guid id)
    {
        var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");
        await _repo.DeleteAsync(camp.Id);
    }
}