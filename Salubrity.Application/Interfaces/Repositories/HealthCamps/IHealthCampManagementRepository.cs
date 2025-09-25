using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps;

public interface IHealthCampManagementRepository
{
    Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId, bool group);
    Task<List<CampPatientSummaryDto>> GetCampPatientsAsync(Guid healthCampId);
    Task<List<CampDailyActivityDto>> GetActivitySummaryAsync(Guid healthCampId);
    Task<List<CampBillingSummaryDto>> GetBillingSummaryAsync(Guid healthCampId);
    Task<HealthCamp?> GetWithDetailsAsync(Guid id);

}
