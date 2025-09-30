using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Application.Interfaces.Repositories.IntakeForms;

public interface IIntakeFormResponseRepository
{
    /// <summary>
    /// Persists a new intake form response (including its field responses).
    /// </summary>
    Task<IntakeFormResponse> AddAsync(IntakeFormResponse response, CancellationToken ct = default);

    /// <summary>
    /// Fetches an intake form response including its field responses.
    /// </summary>
    Task<IntakeFormResponse?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Checks whether a given intake form version exists.
    /// </summary>
    Task<bool> IntakeFormVersionExistsAsync(Guid versionId, CancellationToken ct = default);

    /// <summary>
    /// Returns all field IDs associated with a given form version.
    /// </summary>
    Task<HashSet<Guid>> GetFieldIdsForVersionAsync(Guid versionId, CancellationToken ct = default);
    Task<Guid> GetStatusIdByNameAsync(string name, CancellationToken ct = default);
    Task<List<IntakeFormResponseDetailDto>> GetResponsesByPatientAndCampIdAsync(Guid? patientId, Guid healthCampId, CancellationToken ct = default);

    Task<List<IntakeFormResponse>> GetResponsesByCampIdWithDetailsAsync(Guid campId, CancellationToken ct = default);
    Task<List<IntakeFormResponse>> GetByCampAndAssignmentAsync(
    Guid campId,
    Guid assignmentId,
    PackageItemType assignmentType,
    CancellationToken ct = default);

    // Download Findings Implementation
    Task<List<IntakeFormResponse>> GetResponsesByCampIdWithDetailAsync(Guid campId, CancellationToken ct = default);
}
