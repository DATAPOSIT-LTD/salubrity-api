using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampServiceAssignmentRepository
{
    Task<HealthCampServiceAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<HealthCampServiceAssignment>> GetBySubcontractorIdAsync(Guid subcontractorId, CancellationToken ct = default);
    Task<List<HealthCampServiceAssignment>> GetByCampIdAsync(Guid campId, CancellationToken ct = default);
}
