using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Services.HealthCamps;

namespace Salubrity.Application.Services.HealthCamps
{
    public class HealthCampOverviewService : IHealthCampOverviewService
    {
        private readonly IHealthCampOverviewRepository _repo;

        public HealthCampOverviewService(IHealthCampOverviewRepository repo)
        {
            _repo = repo;
        }

        public async Task<HealthCampOverviewDto> GetHealthCampOverviewAsync()
        {
            return await _repo.GetHealthCampOverviewAsync();
        }

        public async Task<PatientCampOverviewDto> GetPatientCampOverviewAsync(Guid patientId, CancellationToken ct = default)
        {
            return await _repo.GetPatientCampOverviewAsync(patientId, ct);
        }
    }
}
