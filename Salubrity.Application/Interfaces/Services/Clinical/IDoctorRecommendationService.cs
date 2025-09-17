// File: Application/Interfaces/Services/Clinical/IDoctorRecommendationService.cs
using Salubrity.Application.DTOs.Clinical;

namespace Salubrity.Application.Interfaces.Services.Clinical
{
    public interface IDoctorRecommendationService
    {
        Task<DoctorRecommendationResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<DoctorRecommendationResponseDto>> GetByPatientAsync(Guid patientId, CancellationToken ct = default);
        Task<Guid> CreateAsync(CreateDoctorRecommendationDto dto, Guid doctorId, CancellationToken ct = default);
        Task UpdateAsync(UpdateDoctorRecommendationDto dto, CancellationToken ct = default);
        Task DeleteAsync(Guid id, CancellationToken ct = default);
    }
}
