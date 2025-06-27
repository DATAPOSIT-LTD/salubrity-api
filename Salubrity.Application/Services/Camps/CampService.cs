using AutoMapper;
using Salubrity.Application.DTOs.Camps;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Services.Camps;
using Salubrity.Domain.Entities.Camps;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Camps;

public class CampService : ICampService
{
    private readonly ICampRepository _repo;
    private readonly IMapper _mapper;

    public CampService(ICampRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<CampDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<List<CampDto>>(items);
    }

    public async Task<CampDto> GetByIdAsync(Guid id)
    {
        var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");
        return _mapper.Map<CampDto>(camp);
    }

    public async Task<CampDto> CreateAsync(CreateCampDto dto)
    {
        var entity = _mapper.Map<Camp>(dto);
        entity.Id = Guid.NewGuid();
        entity.CreatedAt = DateTime.UtcNow;
        entity.IsActive = true;

        await _repo.CreateAsync(entity);
        return _mapper.Map<CampDto>(entity);
    }

    public async Task<CampDto> UpdateAsync(Guid id, UpdateCampDto dto)
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
        return _mapper.Map<CampDto>(camp);
    }

    //public async Task DeleteAsync(Guid id)
    //{
    //    var camp = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Camp not found");
    //    await _repo.DeleteAsync(camp);
    //}
}
