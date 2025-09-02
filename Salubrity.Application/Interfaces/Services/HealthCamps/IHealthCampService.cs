using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.HealthCamps;

public interface IHealthCampService
{
   Task<List<HealthCampListDto>> GetAllAsync();
   Task<HealthCampDetailDto> GetByIdAsync(Guid id);

   Task<HealthCampDto> CreateAsync(CreateHealthCampDto dto);
   Task<HealthCampDto> UpdateAsync(Guid id, UpdateHealthCampDto dto);
   Task DeleteAsync(Guid id);
   Task<LaunchHealthCampResponseDto> LaunchAsync(LaunchHealthCampDto dto);

   // These now accept nullable Guid?
   Task<List<HealthCampListDto>> GetMyUpcomingCampsAsync(Guid? subcontractorId);
   Task<List<HealthCampListDto>> GetMyCompleteCampsAsync(Guid? subcontractorId);
   Task<List<HealthCampListDto>> GetMyCanceledCampsAsync(Guid? subcontractorId);

   Task<List<CampParticipantListDto>> GetCampParticipantsAllAsync(Guid campId, string? q, string? sort, int page, int pageSize);
   Task<List<CampParticipantListDto>> GetCampParticipantsServedAsync(Guid campId, string? q, string? sort, int page, int pageSize);
   Task<List<CampParticipantListDto>> GetCampParticipantsNotSeenAsync(Guid campId, string? q, string? sort, int page, int pageSize);

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
}
