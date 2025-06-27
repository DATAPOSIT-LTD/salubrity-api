using AutoMapper;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.HealthcareServices;

public class ServiceSubcategoryService : IServiceSubcategoryService
{
    private readonly IServiceSubcategoryRepository _repo;
    private readonly IMapper _mapper;

    public ServiceSubcategoryService(IServiceSubcategoryRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<ServiceSubcategoryResponseDto>> GetAllAsync()
    {
        var entities = await _repo.GetAllAsync();
        return _mapper.Map<List<ServiceSubcategoryResponseDto>>(entities);
    }

    public async Task<ServiceSubcategoryResponseDto> GetByIdAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Subcategory not found.");
        return _mapper.Map<ServiceSubcategoryResponseDto>(entity);
    }

    public async Task<ServiceSubcategoryResponseDto> CreateAsync(CreateServiceSubcategoryDto dto)
    {
        var entity = _mapper.Map<ServiceSubcategory>(dto);
        await _repo.AddAsync(entity);
        return _mapper.Map<ServiceSubcategoryResponseDto>(entity);
    }

    public async Task<ServiceSubcategoryResponseDto> UpdateAsync(Guid id, UpdateServiceSubcategoryDto dto)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Subcategory not found.");
        _mapper.Map(dto, entity);
        await _repo.UpdateAsync(entity);
        return _mapper.Map<ServiceSubcategoryResponseDto>(entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Subcategory not found.");
        await _repo.DeleteAsync(entity);
    }
}
