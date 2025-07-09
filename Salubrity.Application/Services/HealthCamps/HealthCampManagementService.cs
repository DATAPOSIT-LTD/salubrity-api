using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;

namespace Salubrity.Application.Services.HealthCamps;

public class HealthCampManagementService : IHealthCampManagementService
{
    private readonly IHealthCampManagementRepository _repo;

    public HealthCampManagementService(IHealthCampManagementRepository repo)
    {
        _repo = repo;
    }

    public Task<List<ServiceStationSummaryDto>> GetServiceStationsAsync(Guid healthCampId)
        => _repo.GetServiceStationsAsync(healthCampId);

    public Task<List<CampPatientSummaryDto>> GetCampPatientsAsync(Guid healthCampId)
        => _repo.GetCampPatientsAsync(healthCampId);

    public Task<List<CampDailyActivityDto>> GetActivitySummaryAsync(Guid healthCampId)
        => _repo.GetActivitySummaryAsync(healthCampId);

    public Task<List<CampBillingSummaryDto>> GetBillingSummaryAsync(Guid healthCampId)
        => _repo.GetBillingSummaryAsync(healthCampId);
}
