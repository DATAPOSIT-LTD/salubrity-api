using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampManagementRepository
{
    Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId);
    Task<List<CampPatientSummaryDto>> GetCampPatientsAsync(Guid healthCampId);
    Task<List<CampDailyActivityDto>> GetActivitySummaryAsync(Guid healthCampId);
    Task<List<CampBillingSummaryDto>> GetBillingSummaryAsync(Guid healthCampId);
}
