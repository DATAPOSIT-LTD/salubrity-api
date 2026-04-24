// File: Application/Services/Clinical/ServiceReferralService.cs
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Repositories.Clinical;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Domain.Entities.Clinical;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Clinical
{
    public class ServiceReferralService : IServiceReferralService
    {
        private readonly IServiceReferralRepository _repo;

        public ServiceReferralService(IServiceReferralRepository repo)
        {
            _repo = repo;
        }

        public async Task<ServiceReferralResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            return entity == null ? null : MapToDto(entity);
        }

        public async Task<IReadOnlyList<ServiceReferralResponseDto>> GetByParticipantAndCampAsync(Guid participantId, Guid healthCampId, CancellationToken ct = default)
        {
            var list = await _repo.GetByParticipantAndCampAsync(participantId, healthCampId, ct);
            return list.Select(MapToDto).ToList();
        }

        public async Task<IReadOnlyList<ServiceReferralResponseDto>> GetByCampAsync(Guid healthCampId, CancellationToken ct = default)
        {
            var list = await _repo.GetByCampAsync(healthCampId, ct);
            return list.Select(MapToDto).ToList();
        }

        public async Task<Guid> CreateAsync(CreateServiceReferralDto dto, Guid serviceProviderId, CancellationToken ct = default)
        {
            var campId = await _repo.GetHealthCampIdForAssignmentAsync(dto.ServiceAssignmentId, ct)
                ?? throw new NotFoundException("Service assignment not found");

            var entity = new ServiceReferral
            {
                Id = Guid.NewGuid(),
                ParticipantId = dto.ParticipantId,
                HealthCampId = campId,
                ServiceAssignmentId = dto.ServiceAssignmentId,
                ServiceProviderId = serviceProviderId,
                Reason = dto.Reason,
                UrgencyId = dto.UrgencyId,
                FollowUpScheduleId = dto.FollowUpScheduleId,
                CreatedBy = serviceProviderId,
            };

            var created = await _repo.CreateAsync(entity, ct);
            return created.Id;
        }

        public async Task UpdateAsync(Guid id, UpdateServiceReferralDto dto, Guid currentUserId, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Referral not found");

            // TODO: broaden to "any provider assigned to this station". For now, creator-only.
            if (existing.ServiceProviderId != currentUserId)
                throw new UnauthorizedException("You can only edit referrals you created.");

            existing.Reason = dto.Reason;
            existing.UrgencyId = dto.UrgencyId;
            existing.FollowUpScheduleId = dto.FollowUpScheduleId;
            existing.UpdatedAt = DateTime.UtcNow;
            existing.UpdatedBy = currentUserId;

            await _repo.UpdateAsync(existing, ct);
        }

        public async Task DeleteAsync(Guid id, Guid currentUserId, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(id, ct)
                ?? throw new NotFoundException("Referral not found");

            // TODO: broaden to "any provider assigned to this station". For now, creator-only.
            if (existing.ServiceProviderId != currentUserId)
                throw new UnauthorizedException("You can only delete referrals you created.");

            await _repo.DeleteAsync(id, ct);
        }

        private static ServiceReferralResponseDto MapToDto(ServiceReferral e) => new()
        {
            Id = e.Id,
            ParticipantId = e.ParticipantId,
            HealthCampId = e.HealthCampId,
            ServiceAssignmentId = e.ServiceAssignmentId,
            ServiceName = e.ResolvedServiceName ?? string.Empty,
            ServiceProviderId = e.ServiceProviderId,
            ServiceProviderName = e.ServiceProviderFullName ?? string.Empty,
            Speciality = e.Speciality ?? string.Empty,
            Reason = e.Reason,
            Urgency = new BaseLookupResponse { Id = e.Urgency.Id, Name = e.Urgency.Name },
            FollowUpSchedule = new BaseLookupResponse { Id = e.FollowUpSchedule.Id, Name = e.FollowUpSchedule.Name },
            CreatedAt = e.CreatedAt,
        };
    }
}
