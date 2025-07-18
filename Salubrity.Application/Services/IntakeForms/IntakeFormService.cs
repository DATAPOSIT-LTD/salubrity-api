using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Services.IntakeForms;

public class IntakeFormService : IIntakeFormService
{
    private readonly IIntakeFormRepository _repo;

    public IntakeFormService(IIntakeFormRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<IntakeFormDto>> GetAllAsync()
    {
        var forms = await _repo.GetAllAsync();
        return forms.Select(f => new IntakeFormDto
        {
            Id = f.Id,
            Title = f.Title,
            Description = f.Description
        }).ToList();
    }

    public async Task<IntakeFormDto?> GetByIdAsync(Guid id)
    {
        var form = await _repo.GetByIdAsync(id);
        if (form == null) return null;

        return new IntakeFormDto
        {
            Id = form.Id,
            Title = form.Title,
            Description = form.Description
        };
    }

    public async Task<Guid> CreateAsync(CreateIntakeFormDto dto)
    {
        var form = new IntakeForm
        {
            Title = dto.Title,
            Description = dto.Description
        };

        await _repo.AddAsync(form);    ;

        return form.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateIntakeFormDto dto)
    {
        var form = await _repo.GetByIdAsync(id);
        if (form == null) return false;

        form.Title = dto.Title;
        form.Description = dto.Description;

        await _repo.UpdateAsync(form);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var form = await _repo.GetByIdAsync(id);
        if (form == null) return false;

        await _repo.DeleteAsync(form);

        return true;
    }
}
