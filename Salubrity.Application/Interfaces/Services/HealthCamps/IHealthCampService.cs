using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Enums;

namespace Salubrity.Application.Interfaces.Services.HealthCamps;

public interface IHealthCampService
{
    Task<List<HealthCampListDto>> GetAllAsync();
    Task<HealthCampDetailDto> GetByIdAsync(Guid id);

    Task<HealthCampDto> CreateAsync(CreateHealthCampDto dto);
    Task<HealthCampDto> UpdateAsync(Guid id, UpdateHealthCampDto dto);
    Task DeleteAsync(Guid id, Guid userId);
    Task<LaunchHealthCampResponseDto> LaunchAsync(LaunchHealthCampDto dto);
    Task CancelAsync(Guid campId);

    // These now accept nullable Guid?
    Task<List<HealthCampListDto>> GetMyUpcomingCampsAsync(Guid? subcontractorId, CancellationToken ct);
    Task<List<HealthCampListDto>> GetMyCompleteCampsAsync(Guid? subcontractorId);
    Task<List<HealthCampListDto>> GetMyCanceledCampsAsync(Guid? subcontractorId);

    public Task<PagedResult<CampParticipantListDto>> GetCampParticipantsPagedAsync(
         Guid campId,
         Guid? serviceId,          // REQUIRED: station / service context
         Guid? participantId,    // Filter by participant
         CampParticipantServeStatus status, // All | Served | NotSeen
         string? q,
         string? sort,
         int page,
         int pageSize,
         CancellationToken ct = default
     );



    Task<QrEncodingDetailDto> DecodePosterTokenAsync(string token, CancellationToken ct);

    // This also needs to support nullable
    Task<List<HealthCampWithRolesDto>> GetMyCampsWithRolesByStatusAsync(Guid? subcontractorId, string status, CancellationToken ct);

    Task<List<HealthCampPatientDto>> GetCampPatientsByStatusAsync(
        Guid campId,
        string filter,
        string? q,
        string? sort,
        int page,
        int pageSize,
        CancellationToken ct = default);

    Task<CampPatientDetailWithFormsDto> GetCampPatientDetailWithFormsForCurrentAsync(
        Guid campId,
        Guid participantId,
        Guid? subcontractorIdOrNullForAdmin,
        CancellationToken ct = default);

    Task<List<OrganizationCampListDto>> GetCampsByOrganizationAsync(Guid organizationId, CancellationToken ct = default);
    Task<OrganizationStatsDto> GetOrganizationStatsAsync(Guid organizationId, CancellationToken ct = default);
    Task<List<DateTime>> GetUpcomingCampDatesAsync(CancellationToken ct = default);
    Task<CampLinkResultDto> LinkUserToCampAsync(Guid userId, Guid campId, CancellationToken ct = default);

    // IHealthCampService.cs
    Task<CampLinkResultDto> LinkUserToCampByIdAsync(Guid userId, Guid campId, CancellationToken ct = default);
    Task UpdateParticipantBillingStatusAsync(Guid campId, Guid participantId, UpdateParticipantBillingStatusDto dto, CancellationToken ct = default);
    Task<ParticipantBillingStatusDto> GetParticipantBillingStatusAsync(Guid campId, Guid participantId, CancellationToken ct = default);
    Task<List<HealthCampListDto>> GetMyOngoingCampsAsync(Guid? subcontractorId, CancellationToken ct = default);
    Task<List<HealthCampListDto>> GetAdminBillingCampsAsync(CancellationToken ct = default);
    Task AddSubcontractorToCampAsync(Guid campId, ModifySubcontractorCampDto dto, Guid actingUserId);
    Task RemoveSubcontractorFromCampAsync(Guid campId, Guid subcontractorId, Guid actingUserId);
    Task AssignPackageToParticipantAsync(AssignParticipantPackageDto dto, CancellationToken ct);
    Task<List<Salubrity.Application.DTOs.HealthCamps.CampBillingItemDto>> GetCampBillingAsync(Guid campId, CancellationToken ct = default);
    Task<Salubrity.Application.DTOs.HealthCamps.BulkAssignPackageResultDto> BulkAssignPackageAsync(Guid campId, Salubrity.Application.DTOs.HealthCamps.BulkAssignPackageDto dto, CancellationToken ct = default);
    Task<List<HealthCampPackageDto>> GetAllPackagesByCampAsync(Guid campId, CancellationToken ct);

    Task<Salubrity.Application.DTOs.HealthCamps.PublishFinalReportsResultDto> PublishFinalReportsAsync(Guid campId, Guid currentUserId, CancellationToken ct = default);


}
