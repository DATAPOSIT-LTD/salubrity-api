using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Repositories.HealthCamps
{
    public interface IHealthCampOverviewRepository
    {
        Task<HealthCampOverviewDto> GetHealthCampOverviewAsync();

        /// <summary>
        /// Get camp overview for a specific patient: camps attended (completed) and upcoming camps they are registered for.
        /// </summary>
        Task<PatientCampOverviewDto> GetPatientCampOverviewAsync(Guid patientId, CancellationToken ct = default);
    }
}
