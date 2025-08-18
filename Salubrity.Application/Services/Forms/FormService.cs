using AutoMapper;
using Microsoft.Extensions.Logging;
using Salubrity.Application.DTOs.Forms;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Forms;

public class FormService : IFormService
{
    private readonly IIntakeFormRepository _formRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<FormService> _logger;

    public FormService(IIntakeFormRepository formRepo, IMapper mapper, ILogger<FormService> logger)
    {
        _formRepo = formRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FormResponseDto> CreateAsync(CreateFormDto dto)
    {
        var form = _mapper.Map<IntakeForm>(dto);
        form.Id = Guid.NewGuid();
        form.Sections ??= [];

        // Handle direct fields (i.e. if form has fields but no sections)
        if (dto.Fields != null && dto.Fields.Count != 0)
        {
            form.Fields = [.. dto.Fields.Select(fieldDto =>
            {
                var field = _mapper.Map<IntakeFormField>(fieldDto);
                field.FormId = form.Id;
                field.Id = Guid.NewGuid();
                return field;
            })];
        }

        var created = await _formRepo.CreateAsync(form);
        return _mapper.Map<FormResponseDto>(created);
    }


    public async Task<List<FormResponseDto>> GetAllAsync()
    {
        var forms = await _formRepo.GetAllAsync();
        return _mapper.Map<List<FormResponseDto>>(forms);
    }

    public async Task<FormResponseDto> GetByIdAsync(Guid id)
    {
        var form = await _formRepo.GetWithSectionsAsync(id)
                   ?? throw new NotFoundException("Form not found.");

        // Order top-level fields (if any)
        form.Fields = [.. (form.Fields ?? []).OrderBy(static f => f.Order)];

        // Order sections and their fields (if any)
        form.Sections = [.. (form.Sections ?? [])
            .OrderBy(static s => s.Order)
            .Select(static s =>
            {
                s.Fields = [.. (s.Fields ?? []).OrderBy(static f => f.Order)];
                return s;
            })];

        var dto = _mapper.Map<FormResponseDto>(form);

        if (form.Sections != null && form.Sections.Count != 0)
        {
            dto.Fields = []; // clear top-level fields
        }
        else if (form.Fields != null && form.Fields.Count != 0)
        {
            dto.Sections = []; // clear sections
        }

        return dto;
    }


    public async Task<FormResponseDto> UpdateAsync(Guid id, UpdateFormDto dto)
    {
        var form = await _formRepo.GetWithFieldsAsync(id)
                   ?? throw new NotFoundException("Form not found.");

        _mapper.Map(dto, form);
        await _formRepo.UpdateAsync(form);
        return _mapper.Map<FormResponseDto>(form);
    }

    public async Task DeleteAsync(Guid id)
    {
        var exists = await _formRepo.FormExistsAsync(id);
        if (!exists)
            throw new NotFoundException("Form not found.");

        await _formRepo.DeleteAsync(id);
    }
}
