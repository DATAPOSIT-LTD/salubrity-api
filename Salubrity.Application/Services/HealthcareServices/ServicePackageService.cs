using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.HealthcareServices;

public class ServicePackageService : IServicePackageService
{
    private readonly IServicePackageRepository _repository;

    public ServicePackageService(IServicePackageRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ServicePackageResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return [.. items.Select(x => MapToDto(x))];
    }

    public async Task<ServicePackageResponseDto> GetByIdAsync(Guid id)
    {
        var item = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Service package not found.");
        return MapToDto(item);
    }

    public async Task<ServicePackageResponseDto> CreateAsync(CreateServicePackageDto dto)
    {
        if (await _repository.ExistsByNameAsync(dto.Name))
            throw new ValidationException(["Package with the same name already exists."]);

        var entity = new ServicePackage
        {
            Name = dto.Name,
            Description = dto.Description,
            //ServiceSubcategoryIds = dto.ServiceSubcategoryIds,
            Price = dto.Price,
            RangeOfPeople = dto.RangeOfPeople
        };

        await _repository.AddAsync(entity);
        return MapToDto(entity);
    }

    public async Task<ServicePackageResponseDto> UpdateAsync(Guid id, UpdateServicePackageDto dto)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Service package not found.");

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        //entity.ServiceSubcategoryIds = dto.ServiceSubcategoryIds;
        entity.Price = dto.Price;
        entity.RangeOfPeople = dto.RangeOfPeople;
        entity.IsActive = dto.IsActive;

        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Service package not found.");

        await _repository.DeleteAsync(entity);
    }

    private static ServicePackageResponseDto MapToDto(ServicePackage e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        Description = e.Description,
        //ServiceSubcategoryIds = e.ServiceSubcategoryIds,
        Price = e.Price,
        RangeOfPeople = e.RangeOfPeople,
        IsActive = e.IsActive
    };
}
