using Salubrity.Domain.Entities.Subcontractor;

namespace Salubrity.Application.Interfaces.Repositories
{
    public interface ISubcontractorRepository
    {
        Task AddAsync(Subcontractor entity);
        Task<Subcontractor?> GetByIdAsync(Guid id);
        Task<List<Subcontractor>> GetAllWithDetailsAsync();

        Task<Subcontractor?> GetByIdWithDetailsAsync(Guid id);
        Task AssignRoleAsync(Guid subcontractorId, Guid roleId, bool isPrimary, CancellationToken ct = default);
        Task AssignSpecialtyAsync(Guid subcontractorId, Guid serviceId);
        Task SaveChangesAsync();
    }
}
