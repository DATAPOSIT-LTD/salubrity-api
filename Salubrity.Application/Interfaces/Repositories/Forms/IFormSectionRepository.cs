// File: Salubrity.Application/Interfaces/Repositories/Forms/IFormSectionRepository.cs
using Salubrity.Domain.Entities.FormSections;

namespace Salubrity.Application.Interfaces.Repositories.Forms;

public interface IFormSectionRepository
{
    Task<FormSection?> GetByIdAsync(Guid sectionId);
    Task<List<FormSection>> GetByFormIdAsync(Guid formId);
    Task<FormSection> CreateAsync(FormSection section);
    Task<FormSection> UpdateAsync(FormSection section);
    Task DeleteAsync(Guid sectionId);
}
