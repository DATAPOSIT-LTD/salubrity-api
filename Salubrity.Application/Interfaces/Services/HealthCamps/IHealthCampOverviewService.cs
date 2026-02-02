using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.HealthCamps
{
    public interface IHealthCampOverviewService
    {
        Task<HealthCampOverviewDto> GetHealthCampOverviewAsync();

        /// <summary>
        /// Get camp overview for a patient: camps attended and upcoming camps (by patientId from /me).
        /// </summary>
        Task<PatientCampOverviewDto> GetPatientCampOverviewAsync(Guid patientId, CancellationToken ct = default);
    }
}
