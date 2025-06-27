using AutoMapper;
using Salubrity.Application.DTOs.HealthcareServices;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Application.Interfaces.Services.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Shared.Exceptions;


namespace Salubrity.Application.Services.HealthcareServices;


public class IndustryService : IIndustryService
{
    private readonly IIndustryRepository _repo;
    private readonly IMapper _mapper;

    public IndustryService(IIndustryRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<List<IndustryResponseDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return _mapper.Map<List<IndustryResponseDto>>(items);
    }

    public async Task<IndustryResponseDto> GetByIdAsync(Guid id)
    {
        var item = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Industry not found");
        return _mapper.Map<IndustryResponseDto>(item);
    }

    public async Task<IndustryResponseDto> CreateAsync(CreateIndustryDto dto)
    {
        //if (await _repo.ExistsByNameAsync(dto.Name))
        //    throw new ValidationException("Industry name already exists");

        var entity = _mapper.Map<Industry>(dto);
        await _repo.AddAsync(entity);
        return _mapper.Map<IndustryResponseDto>(entity);
    }

    public async Task<IndustryResponseDto> UpdateAsync(Guid id, UpdateIndustryDto dto)
    {
        var existing = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Industry not found");

        if (dto.Name != null) existing.Name = dto.Name;
        if (dto.Description != null) existing.Description = dto.Description;
        if (dto.IsActive.HasValue) existing.IsActive = dto.IsActive.Value;

        await _repo.UpdateAsync(existing);
        return _mapper.Map<IndustryResponseDto>(existing);
    }

    public async Task DeleteAsync(Guid id)
    {
        var existing = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Industry not found");
        await _repo.DeleteAsync(existing);
    }
}
