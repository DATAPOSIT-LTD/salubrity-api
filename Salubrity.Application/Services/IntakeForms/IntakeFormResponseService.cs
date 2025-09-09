#nullable enable
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Forms;

public sealed class IntakeFormResponseService : IIntakeFormResponseService
{
    private readonly IIntakeFormResponseRepository _intakeFormResponseRepository;
    private readonly IHealthCampParticipantRepository _participantRepository;
    private readonly ICampQueueRepository _stationCheckInRepository;

    public IntakeFormResponseService(IIntakeFormResponseRepository intakeFormResponseRepository, IHealthCampParticipantRepository participantRepository, ICampQueueRepository campQueueRepository)
    {
        _intakeFormResponseRepository = intakeFormResponseRepository;
        _participantRepository = participantRepository;
        _stationCheckInRepository = campQueueRepository;
    }


    // public async Task<Guid> SubmitResponseAsync(CreateIntakeFormResponseDto dto, Guid submittedByUserId, CancellationToken ct = default)
    // {
    //     var versionExists = await _intakeFormResponseRepository.IntakeFormVersionExistsAsync(dto.IntakeFormVersionId, ct);
    //     if (!versionExists)
    //         throw new NotFoundException("Form version not found.");

    //     var validFieldIds = await _intakeFormResponseRepository.GetFieldIdsForVersionAsync(dto.IntakeFormVersionId, ct);
    //     var invalidField = dto.FieldResponses.FirstOrDefault(f => !validFieldIds.Contains(f.FieldId));
    //     if (invalidField is not null)
    //         throw new ValidationException([$"Field {invalidField.FieldId} does not belong to form version {dto.IntakeFormVersionId}."]);

    //     // Lookup patient ID from participant ID
    //     var patientId = await _participantRepository.GetPatientIdByParticipantIdAsync(dto.PatientId, ct);
    //     if (patientId is null)
    //         throw new NotFoundException($"Patient not found for participant {dto.PatientId}.");

    //     // Lookup default status by name if not provided
    //     var statusId = dto.ResponseStatusId
    //         ?? await _intakeFormResponseRepository.GetStatusIdByNameAsync("Submitted", ct);

    //     var response = new IntakeFormResponse
    //     {
    //         Id = Guid.NewGuid(),
    //         IntakeFormVersionId = dto.IntakeFormVersionId,
    //         SubmittedByUserId = submittedByUserId,
    //         PatientId = patientId.Value,
    //         ServiceId = dto.ServiceId,
    //         ResponseStatusId = statusId,
    //         FieldResponses = dto.FieldResponses.Select(f => new IntakeFormFieldResponse
    //         {
    //             Id = Guid.NewGuid(),
    //             FieldId = f.FieldId,
    //             Value = f.Value
    //         }).ToList()
    //     };

    //     await _intakeFormResponseRepository.AddAsync(response, ct);
    //     return response.Id;
    // }


    public async Task<Guid> SubmitResponseAsync(CreateIntakeFormResponseDto dto, Guid submittedByUserId, CancellationToken ct = default)
    {
        // 1) Validate form version
        var versionExists = await _intakeFormResponseRepository.IntakeFormVersionExistsAsync(dto.IntakeFormVersionId, ct);
        if (!versionExists)
            throw new NotFoundException("Form version not found.");

        // 2) Validate fields belong to the version
        var validFieldIds = await _intakeFormResponseRepository.GetFieldIdsForVersionAsync(dto.IntakeFormVersionId, ct);
        var invalidField = dto.FieldResponses.FirstOrDefault(f => !validFieldIds.Contains(f.FieldId));
        if (invalidField is not null)
            throw new ValidationException([$"Field {invalidField.FieldId} does not belong to form version {dto.IntakeFormVersionId}."]);

        // 3) Resolve patient from participant
        var patientId = await _participantRepository.GetPatientIdByParticipantIdAsync(dto.PatientId, ct);
        if (patientId is null)
            throw new NotFoundException($"Patient not found for participant {dto.PatientId}.");

        // 4) Resolve response status (default: Submitted)
        var statusId = dto.ResponseStatusId
            ?? await _intakeFormResponseRepository.GetStatusIdByNameAsync("Submitted", ct);

        // 5) Create response aggregate
        var response = new IntakeFormResponse
        {
            Id = Guid.NewGuid(),
            IntakeFormVersionId = dto.IntakeFormVersionId,
            SubmittedByUserId = submittedByUserId,
            PatientId = patientId.Value,
            ServiceId = dto.ServiceId,
            ResponseStatusId = statusId,
            FieldResponses = dto.FieldResponses.Select(f => new IntakeFormFieldResponse
            {
                Id = Guid.NewGuid(),
                FieldId = f.FieldId,
                Value = f.Value
            }).ToList()
        };

        // 6) Persist response
        await _intakeFormResponseRepository.AddAsync(response, ct);

        // 7) Checkout logic — mark station as Completed
        HealthCampStationCheckIn? checkIn = null;

        if (dto.StationCheckInId is Guid explicitCheckInId)
        {
            checkIn = await _stationCheckInRepository.GetByIdAsync(explicitCheckInId, ct);
            if (checkIn is null)
                throw new NotFoundException($"Station check-in not found: {explicitCheckInId}");
        }
        else
        {
            // Use Participant + (optional) Assignment from DTO to find the active check-in
            checkIn = await _stationCheckInRepository.GetActiveForParticipantAsync(
                dto.PatientId,                // this is participantId in your code path
                dto.HealthCampServiceAssignmentId, // may be null -> take the most relevant active
                ct);
        }

        if (checkIn is not null)
        {
            if (checkIn.Status is CampQueueStatus.Canceled or CampQueueStatus.Completed)
                throw new ValidationException([$"Cannot complete a check-in in '{checkIn.Status}' status."]);

            checkIn.Status = CampQueueStatus.Completed;
            var now = DateTimeOffset.UtcNow;
            checkIn.FinishedAt = now;
            checkIn.StartedAt ??= now; // if it was never moved to InService, at least stamp a start

            await _stationCheckInRepository.UpdateAsync(checkIn, ct);
        }
        else
        {
            // Optional: choose strict vs lenient behavior
            // Strict = throw; Lenient = silently allow response without check-out.
            // throw new NotFoundException("No active station check-in found for this participant.");
        }

        return response.Id;
    }


    public async Task<IntakeFormResponseDto?> GetAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _intakeFormResponseRepository.GetByIdAsync(id, ct);
        if (entity is null) return null;

        return new IntakeFormResponseDto
        {
            Id = entity.Id,
            IntakeFormVersionId = entity.IntakeFormVersionId,
            SubmittedByUserId = entity.SubmittedByUserId,
            PatientId = entity.PatientId,
            ServiceId = entity.ServiceId,
            ResponseStatusId = entity.ResponseStatusId,
            FieldResponses = entity.FieldResponses
                .OrderBy(f => f.CreatedAt)
                .Select(f => new IntakeFormFieldResponseDto
                {
                    Id = f.Id,
                    ResponseId = f.ResponseId,
                    FieldId = f.FieldId,
                    Value = f.Value
                }).ToList()
        };
    }


    public async Task<List<IntakeFormResponseDetailDto>> GetResponsesByPatientAndCampIdAsync(Guid patientId, Guid healthCampId, CancellationToken ct = default)
    {
        var responses = await _intakeFormResponseRepository.GetResponsesByPatientAndCampIdAsync(patientId, healthCampId, ct);

        if (!responses.Any())
            throw new NotFoundException("No responses found for this patient in this camp.");

        return responses;
    }

}
