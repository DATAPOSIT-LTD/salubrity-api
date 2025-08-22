using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.HealthCamps;

public interface IHealthCampManagementService
{
    /// <summary>
    /// Returns a list of service stations for the given health camp.
    /// </summary>
    Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId);

    /// <summary>
    /// Returns a list of patients participating in the given health camp.
    /// </summary>
    Task<List<CampPatientSummaryDto>> GetCampPatientsAsync(Guid healthCampId);

    /// <summary>
    /// Returns daily activity summary for the health camp.
    /// </summary>
    Task<List<CampDailyActivityDto>> GetActivitySummaryAsync(Guid healthCampId);

    /// <summary>
    /// Returns billing information for the health camp patients.
    /// </summary>
    Task<List<CampBillingSummaryDto>> GetBillingSummaryAsync(Guid healthCampId);
    Task<OrganizationCampDetailsDto> GetOrganizationCampDetailsAsync(Guid healthCampId);


}
