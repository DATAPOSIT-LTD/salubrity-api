using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.HealthCamps;

public interface IHealthCampOverviewService
{
    Task<HealthCampOverviewDto> GetHealthCampOverviewAsync();
    Task<PatientCampOverviewDto> GetPatientCampOverviewAsync(Guid patientId, CancellationToken ct = default);
}
