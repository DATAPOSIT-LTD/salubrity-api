using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Shared.Exceptions;

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
        return [.. forms.Select(f => new IntakeFormDto
        {
            Id = f.Id,
            Name = f.Name,
            Description = f.Description
        })];
    }

    public async Task<IntakeFormDto?> GetByIdAsync(Guid id)
    {
        var form = await _repo.GetByIdAsync(id);
        if (form == null) return null;

        return new IntakeFormDto
        {
            Id = form.Id,
            Name = form.Name,
            Description = form.Description
        };
    }

    public async Task<Guid> CreateAsync(CreateIntakeFormDto dto)
    {
        var form = new IntakeForm
        {
            Name = dto.Name,
            Description = dto.Description
        };

        await _repo.CreateAsync(form); ;

        return form.Id;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateIntakeFormDto dto)
    {
        var form = await _repo.GetByIdAsync(id);
        if (form == null) return false;

        form.Name = dto.Name;
        form.Description = dto.Description;

        await _repo.UpdateAsync(form);

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var form = await _repo.GetByIdAsync(id);
        if (form == null) return false;

        var isAssigned = await _repo.IsFormAssignedAnywhereAsync(id);
        if (isAssigned)
            throw new ValidationException(["This intake form is currently assigned to one or more services and cannot be deleted."]);

        await _repo.DeleteAsync(id);
        return true;
    }

}
