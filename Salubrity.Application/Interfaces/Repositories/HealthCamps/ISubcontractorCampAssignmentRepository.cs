// File: Application/Interfaces/Repositories/HealthCamps/ISubcontractorCampAssignmentRepository.cs
using Salubrity.Domain.Entities.Subcontractor;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface ISubcontractorCampAssignmentRepository
{
    Task AddAsync(SubcontractorHealthCampAssignment assignment, CancellationToken ct = default);
    Task<List<SubcontractorHealthCampAssignment>> GetByCampIdAsync(Guid healthCampId, CancellationToken ct = default);
    Task<bool> ExistsAsync(Guid subcontractorId, Guid healthCampId, CancellationToken ct = default);
    Task<bool> HasActiveAssignmentsAsync(Guid id, CancellationToken ct);
    Task<List<SubcontractorHealthCampAssignment>> GetByCampAndSubcontractorAsync(Guid campId, Guid subcontractorId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
