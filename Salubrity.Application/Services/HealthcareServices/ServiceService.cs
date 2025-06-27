using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.HealthcareServices;


public class ServiceService : IServiceService
{
    private readonly IServiceRepository _repo;

    public ServiceService(IServiceRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ServiceResponseDto>> GetAllAsync()
    {
        var data = await _repo.GetAllAsync();
        return data.Select(x => new ServiceResponseDto
        {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            IndustryId = x.IndustryId,
            IsActive = x.IsActive
        }).ToList();
    }

    public async Task<ServiceResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Service not found");
        return new ServiceResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IndustryId = entity.IndustryId,
            IsActive = entity.IsActive
        };
    }

    public async Task<ServiceResponseDto> CreateAsync(CreateServiceDto input)
    {
        if (await _repo.ExistsByNameAsync(input.Name))
            throw new ValidationException(["Service with same name already exists."]);

        var entity = new Service
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            Description = input.Description,
            IndustryId = input.IndustryId,
            IsActive = true
        };

        await _repo.AddAsync(entity);
        return new ServiceResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IndustryId = entity.IndustryId,
            IsActive = entity.IsActive
        };
    }

    public async Task<ServiceResponseDto> UpdateAsync(Guid id, UpdateServiceDto input)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Service not found");

        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.IndustryId = input.IndustryId;

        await _repo.UpdateAsync(entity);
        return new ServiceResponseDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IndustryId = entity.IndustryId,
            IsActive = entity.IsActive
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Service not found");
        await _repo.DeleteAsync(entity);
    }
}
