using Salubrity.Application.DTOs.HealthCamps;

namespace Salubrity.Application.Interfaces.Services.HealthCamps
{
    public interface IHealthCampOverviewService
    {
        Task<HealthCampOverviewDto> GetHealthCampOverviewAsync();

        /// <summary>
        /// Get camp overview for a user (participant): camps attended and upcoming camps (by userId from /me).
        /// </summary>
        Task<PatientCampOverviewDto> GetPatientCampOverviewAsync(Guid userId, CancellationToken ct = default);
    }
}
