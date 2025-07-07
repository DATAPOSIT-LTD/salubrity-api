using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.HealthcareServices;

public class ServiceCategoryService : IServiceCategoryService
{
    private readonly IServiceCategoryRepository _repository;

    public ServiceCategoryService(IServiceCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ServiceCategoryResponseDto>> GetAllAsync()
    {
        var items = await _repository.GetAllAsync();
        return items.Select(MapToDto).ToList();
    }

    public async Task<ServiceCategoryResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Service category not found.");
        return MapToDto(entity);
    }

    public async Task<ServiceCategoryResponseDto> CreateAsync(CreateServiceCategoryDto dto)
    {
        var entity = new ServiceCategory
        {
            Name = dto.Name,
            Description = dto.Description,
            ServiceId = dto.ServiceId,
            Price = dto.Price,
            DurationMinutes = dto.DurationMinutes,
            IsActive = true
        };

        await _repository.AddAsync(entity);
        return MapToDto(entity);
    }

    public async Task<ServiceCategoryResponseDto> UpdateAsync(Guid id, UpdateServiceCategoryDto dto)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Service category not found.");

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.Price = dto.Price;
        entity.DurationMinutes = dto.DurationMinutes;

        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Service category not found.");
        await _repository.DeleteAsync(entity);
    }

    private static ServiceCategoryResponseDto MapToDto(ServiceCategory entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        ServiceId = entity.ServiceId,
        Price = entity.Price,
        DurationMinutes = entity.DurationMinutes,
        IsActive = entity.IsActive
    };


}
