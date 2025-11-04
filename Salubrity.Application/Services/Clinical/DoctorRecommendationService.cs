// File: Application/Services/Clinical/DoctorRecommendationService.cs
using Salubrity.Application.DTOs.Clinical;
using Salubrity.Application.DTOs.Lookups;
using Salubrity.Application.Interfaces.Repositories.Clinical;
using Salubrity.Application.Interfaces.Services.Clinical;
using Salubrity.Domain.Entities.Clinical;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Clinical
{
    public class DoctorRecommendationService : IDoctorRecommendationService
    {
        private readonly IDoctorRecommendationRepository _repo;

        public DoctorRecommendationService(IDoctorRecommendationRepository repo)
        {
            _repo = repo;
        }

        public async Task<DoctorRecommendationResponseDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var entity = await _repo.GetByIdAsync(id, ct);
            if (entity == null) return null;

            return MapToDto(entity);
        }

        public async Task<IReadOnlyList<DoctorRecommendationResponseDto>> GetByPatientAsync(Guid patientId, CancellationToken ct = default)
        {
            var list = await _repo.GetByPatientAsync(patientId, ct);
            return list.Select(MapToDto).ToList();
        }

        public async Task<Guid> CreateAsync(CreateDoctorRecommendationDto dto, Guid doctorId, CancellationToken ct = default)
        {
            var entity = new DoctorRecommendation
            {
                Id = Guid.NewGuid(),
                PatientId = dto.PatientId,
                HealthCampId = dto.HealthCampId,
                DoctorId = doctorId,

                // new fields
                PertinentHistoryFindings = dto.PertinentHistoryFindings,
                PertinentClinicalFindings = dto.PertinentClinicalFindings,
                DiagnosticImpression = dto.DiagnosticImpression,
                Conclusion = dto.Conclusion,

                FollowUpRecommendationId = dto.FollowUpRecommendationId,
                RecommendationTypeId = dto.RecommendationTypeId,
                Instructions = dto.Instructions
            };

            var created = await _repo.CreateAsync(entity, ct);
            return created.Id;
        }

        public async Task UpdateAsync(UpdateDoctorRecommendationDto dto, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(dto.Id, ct);
            if (existing == null) throw new NotFoundException("DoctorRecommendation not found");

            // update new fields
            existing.PertinentHistoryFindings = dto.PertinentHistoryFindings;
            existing.PertinentClinicalFindings = dto.PertinentClinicalFindings;
            existing.DiagnosticImpression = dto.DiagnosticImpression;
            existing.Conclusion = dto.Conclusion;

            existing.FollowUpRecommendationId = dto.FollowUpRecommendationId;
            existing.RecommendationTypeId = dto.RecommendationTypeId;
            existing.Instructions = dto.Instructions;

            await _repo.UpdateAsync(existing, ct);
        }

        public async Task DeleteAsync(Guid id, CancellationToken ct = default)
        {
            await _repo.DeleteAsync(id, ct);
        }

        private static DoctorRecommendationResponseDto MapToDto(DoctorRecommendation entity)
        {
            return new DoctorRecommendationResponseDto
            {
                Id = entity.Id,
                PatientId = entity.PatientId,
                DoctorId = entity.DoctorId,
                HealthCampId = entity.HealthCampId,

                // map new fields
                PertinentHistoryFindings = entity.PertinentHistoryFindings,
                PertinentClinicalFindings = entity.PertinentClinicalFindings,
                DiagnosticImpression = entity.DiagnosticImpression,
                Conclusion = entity.Conclusion,

                FollowUpRecommendation = new BaseLookupResponse
                {
                    Id = entity.FollowUpRecommendation.Id,
                    Name = entity.FollowUpRecommendation.Name
                },
                RecommendationType = new BaseLookupResponse
                {
                    Id = entity.RecommendationType.Id,
                    Name = entity.RecommendationType.Name
                },
                Instructions = entity.Instructions,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<IReadOnlyList<DoctorRecommendationResponseDto>> GetByHealthCampAsync(Guid healthCampId, CancellationToken ct = default)
        {
            var list = await _repo.GetByHealthCampAsync(healthCampId, ct);
            return list.Select(MapToDto).ToList();
        }
    }
}
