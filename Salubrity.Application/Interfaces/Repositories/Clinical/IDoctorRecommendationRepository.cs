// File: Application/Interfaces/Repositories/Clinical/IDoctorRecommendationRepository.cs
using Salubrity.Domain.Entities.Clinical;

namespace Salubrity.Application.Interfaces.Repositories.Clinical
{
    public interface IDoctorRecommendationRepository
    {
        Task<DoctorRecommendation?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<DoctorRecommendation>> GetByPatientAsync(Guid patientId, CancellationToken ct = default);
        Task<DoctorRecommendation> CreateAsync(DoctorRecommendation entity, CancellationToken ct = default);
        Task<DoctorRecommendation> UpdateAsync(DoctorRecommendation entity, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
