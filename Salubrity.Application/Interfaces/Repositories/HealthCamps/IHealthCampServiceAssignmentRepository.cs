using System.Linq.Expressions;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampServiceAssignmentRepository
{
    Task<HealthCampServiceAssignment?> GetByIdAsync(
        Guid id,
        CancellationToken ct = default);

    Task<List<HealthCampServiceAssignment>> GetBySubcontractorIdAsync(
        Guid subcontractorId,
        CancellationToken ct = default);

    Task<List<HealthCampServiceAssignment>> GetByCampIdAsync(
        Guid campId,
        CancellationToken ct = default);

    Task<HealthCampServiceAssignment?> FirstOrDefaultAsync(
        Expression<Func<HealthCampServiceAssignment, bool>> predicate,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        Guid campId,
        Guid subcontractorId,
        Guid assignmentId,
        CancellationToken ct = default);

    Task AddAsync(
        HealthCampServiceAssignment assignment,
        CancellationToken ct = default);

    Task AddRangeAsync(
        IEnumerable<HealthCampServiceAssignment> assignments,
        CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
    Task<int> SoftDeleteByCampAndSubcontractorAsync(Guid campId, Guid subcontractorId, Guid actingUserId, CancellationToken ct = default);
}
