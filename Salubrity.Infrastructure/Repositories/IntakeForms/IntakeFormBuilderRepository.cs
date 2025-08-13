// File: Salubrity.Infrastructure/Repositories/Forms/FormBuilderRepository.cs
#nullable enable
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.IntakeForms
{
    public class FormBuilderRepository : IFormBuilderRepository
    {
        private readonly AppDbContext _db;

        public FormBuilderRepository(AppDbContext db) => _db = db;

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default)
            => await _db.IntakeForms.AnyAsync(f => f.Name == name, ct);

        public async Task AddFormGraphAsync(IntakeForm form, CancellationToken ct = default)
        {
            // Add the whole graph; FKs must already be set
            await _db.IntakeForms.AddAsync(form, ct);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<IntakeForm?> LoadFormWithFieldsAsync(Guid formId, CancellationToken ct = default)
        {
            return await _db.IntakeForms
                .Include(f => f.Sections)
                    .ThenInclude(s => s.Fields)
                        .ThenInclude(ff => ff.Options)
                .Include(f => f.Fields) // unsectioned
                    .ThenInclude(ff => ff.Options)
                .FirstOrDefaultAsync(f => f.Id == formId, ct);
        }
    }
}
