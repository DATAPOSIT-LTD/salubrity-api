using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Enums;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Join;

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
    Task<List<HealthCamp>> GetMyUpcomingCampsAsync(Guid? subcontractorId, CancellationToken ct = default);
    Task<List<HealthCamp>> GetMyCompleteCampsAsync(Guid subcontractorId, CancellationToken ct = default);
    Task<List<HealthCamp>> GetMyCanceledCampsAsync(Guid subcontractorId, CancellationToken ct = default);
    Task<HealthCamp?> GetBySlugAsync(string slug, CancellationToken ct = default);


    // Admin-wide
    Task<List<HealthCamp>> GetAllUpcomingCampsAsync(CancellationToken ct = default);
    Task<List<HealthCamp>> GetAllOngoingCampsAsync(CancellationToken ct = default);
    Task<List<HealthCamp>> GetAllCompleteCampsAsync(CancellationToken ct = default);
    Task<List<HealthCamp>> GetAllCanceledCampsAsync(CancellationToken ct = default);

    // =====================================================
    // Participants — PUBLIC CONTRACT
    // =====================================================

    // Station / service-scoped view
    Task<PagedResult<CampParticipantListDto>> GetCampParticipantsByServiceAsync(
        Guid campId,
        Guid serviceId,
        Guid? participantId, // Filter by participant
        CampParticipantServeStatus status,
        string? q,
        string? sort,
        int page,
        int pageSize,
        CancellationToken ct = default);

    // Camp-wide admin view


    Task<PagedResult<CampParticipantListDto>> GetCampParticipantsCampWideAsync(
        Guid campId,
        Guid? participantId, // Filter by participant
        CampParticipantServeStatus status,
        string? q,
        string? sort,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<List<HealthCampWithRolesDto>> GetMyCampsWithRolesByStatusAsync(Guid? subcontractorId, string status, CancellationToken ct = default);
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
    Task<OrganizationStatsDto> GetOrganizationStatsAsync(Guid organizationId, CancellationToken ct = default);
    Task<List<DateTime>> GetUpcomingCampDatesAsync(CancellationToken ct = default);
    Task<List<HealthCampParticipant>> GetParticipantsAsync(Guid campId, string? q, string? sort, CancellationToken ct = default);
    Task<HealthCamp?> GetByIdWithPackagesAsync(Guid id, CancellationToken ct = default);

    // MultiCamp batch fetching

    // Add these method signatures
    Task<List<HealthCamp>> GetAllWithDetailsAsync(CancellationToken ct = default);
    Task<Dictionary<Guid, List<HealthCampParticipant>>> GetParticipantsForMultipleCampsAsync(List<Guid> campIds, CancellationToken ct = default);

    Task<List<Salubrity.Application.DTOs.HealthCamps.CampParticipantContactDto>> GetCampParticipantContactsAsync(Guid campId, CancellationToken ct = default);

    Task<List<Salubrity.Application.DTOs.HealthCamps.MyStationAssignmentDto>> GetMyStationAssignmentsAsync(Guid campId, Guid subcontractorId, CancellationToken ct = default);

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
