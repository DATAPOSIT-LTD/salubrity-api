using AutoMapper;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.HealthCamps;

public class HealthCampService : IHealthCampService
{
    private readonly IHealthCampRepository _repo;
    private readonly IMapper _mapper;

    public HealthCampService(IHealthCampRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<HealthCampDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<List<HealthCampDto>>(items);
    }

    public async Task<HealthCampDto> GetByIdAsync(Guid id)
    {
        var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");
        return _mapper.Map<HealthCampDto>(camp);
    }

    public async Task<HealthCampDto> CreateAsync(CreateHealthCampDto dto)
    {
        var entity = _mapper.Map<HealthCamp>(dto);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsActive = true;

        await _repo.CreateAsync(entity);
        return _mapper.Map<HealthCampDto>(entity);
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
