// File: Application/Interfaces/Services/Clinical/IServiceReferralService.cs
using Salubrity.Application.DTOs.Clinical;

namespace Salubrity.Application.Interfaces.Services.Clinical
{
    public interface IServiceReferralService
    {
        Task<ServiceReferralResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<IReadOnlyList<ServiceReferralResponseDto>> GetByParticipantAndCampAsync(Guid participantId, Guid healthCampId, CancellationToken ct = default);
        Task<IReadOnlyList<ServiceReferralResponseDto>> GetByCampAsync(Guid healthCampId, CancellationToken ct = default);
        Task<Guid> CreateAsync(CreateServiceReferralDto dto, Guid serviceProviderId, CancellationToken ct = default);
        Task UpdateAsync(Guid id, UpdateServiceReferralDto dto, Guid currentUserId, CancellationToken ct = default);
        Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct = default);
    }
}
