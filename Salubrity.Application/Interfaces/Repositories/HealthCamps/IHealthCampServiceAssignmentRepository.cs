using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampServiceAssignmentRepository
{
    Task<HealthCampServiceAssignment?> GetByIdAsync(Guid id, CancellationToken ct = default);
}
