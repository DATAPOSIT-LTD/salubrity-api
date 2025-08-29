using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampRepository
{
    Task<List<HealthCampListDto>> GetAllAsync();
    Task<HealthCamp?> GetByIdAsync(Guid id);
    Task<HealthCampDetailDto?> GetCampDetailsByIdAsync(Guid id);
    Task<HealthCamp> CreateAsync(HealthCamp entity);
    Task<HealthCamp> UpdateAsync(HealthCamp entity);
    Task DeleteAsync(Guid id);

    Task<HealthCamp?> GetForLaunchAsync(Guid id);
    Task UpsertTempCredentialAsync(HealthCampTempCredentialUpsert upsert);

    // Subcontractor-scoped
    Task<List<HealthCamp>> GetMyUpcomingCampsAsync(Guid subcontractorId, CancellationToken ct = default);
    Task<List<HealthCamp>> GetMyCompleteCampsAsync(Guid subcontractorId, CancellationToken ct = default);
    Task<List<HealthCamp>> GetMyCanceledCampsAsync(Guid subcontractorId, CancellationToken ct = default);

    // Admin-wide
    Task<List<HealthCamp>> GetAllUpcomingCampsAsync(CancellationToken ct = default);
    Task<List<HealthCamp>> GetAllCompleteCampsAsync(CancellationToken ct = default);
    Task<List<HealthCamp>> GetAllCanceledCampsAsync(CancellationToken ct = default);

    // Participants
    Task<List<CampParticipantListDto>> GetCampParticipantsAllAsync(Guid campId, string? q, string? sort, int page, int pageSize, CancellationToken ct = default);
    Task<List<CampParticipantListDto>> GetCampParticipantsServedAsync(Guid campId, string? q, string? sort, int page, int pageSize, CancellationToken ct = default);
    Task<List<CampParticipantListDto>> GetCampParticipantsNotSeenAsync(Guid campId, string? q, string? sort, int page, int pageSize, CancellationToken ct = default);
    Task<List<HealthCampWithRolesDto>> GetMyCampsWithRolesByStatusAsync(Guid subcontractorId, string status, CancellationToken ct = default);
    Task<List<HealthCampPatientDto>> GetCampPatientsByStatusAsync(
           Guid campId,
           string filter,
           string? q,
           string? sort,
           int page,
           int pageSize,
           CancellationToken ct = default);

    Task<CampPatientDetailWithFormsDto?> GetCampPatientDetailWithFormsAsync(
         Guid campId,
         Guid participantId,
         Guid? subcontractorId, // null => admin (all assignments)
         CancellationToken ct = default);

    // Organization-scoped
    Task<List<OrganizationCampListDto>> GetCampsByOrganizationAsync(Guid organizationId, CancellationToken ct = default);
}


public sealed class HealthCampTempCredentialUpsert
{
    public Guid HealthCampId { get; set; }
    public Guid UserId { get; set; }
    public string Role { get; set; } = null!;
    public string TempPasswordHash { get; set; } = null!;
    public DateTimeOffset TempPasswordExpiresAt { get; set; }
    public string SignInJti { get; set; } = null!;
    public DateTimeOffset TokenExpiresAt { get; set; }
}
