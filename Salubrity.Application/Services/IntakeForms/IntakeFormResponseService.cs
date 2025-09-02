#nullable enable
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Forms;

public sealed class IntakeFormResponseService : IIntakeFormResponseService
{
    private readonly IIntakeFormResponseRepository _intakeFormResponseRepository;
    private readonly IHealthCampParticipantRepository _participantRepository;

    public IntakeFormResponseService(IIntakeFormResponseRepository intakeFormResponseRepository, IHealthCampParticipantRepository participantRepository)
    {
        _intakeFormResponseRepository = intakeFormResponseRepository;
        _participantRepository = participantRepository;
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

    //     //  Lookup default status by name if not provided
    //     var statusId = dto.ResponseStatusId
    //         ?? await _intakeFormResponseRepository.GetStatusIdByNameAsync("Submitted", ct);




    //     var response = new IntakeFormResponse
    //     {
    //         Id = Guid.NewGuid(),
    //         IntakeFormVersionId = dto.IntakeFormVersionId,
    //         SubmittedByUserId = submittedByUserId,
    //         PatientId = dto.PatientId,
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
        var versionExists = await _intakeFormResponseRepository.IntakeFormVersionExistsAsync(dto.IntakeFormVersionId, ct);
        if (!versionExists)
            throw new NotFoundException("Form version not found.");

        var validFieldIds = await _intakeFormResponseRepository.GetFieldIdsForVersionAsync(dto.IntakeFormVersionId, ct);
        var invalidField = dto.FieldResponses.FirstOrDefault(f => !validFieldIds.Contains(f.FieldId));
        if (invalidField is not null)
            throw new ValidationException([$"Field {invalidField.FieldId} does not belong to form version {dto.IntakeFormVersionId}."]);

        // Lookup patient ID from participant ID
        var patientId = await _participantRepository.GetPatientIdByParticipantIdAsync(dto.PatientId, ct);
        if (patientId is null)
            throw new NotFoundException($"Patient not found for participant {dto.PatientId}.");

        // Lookup default status by name if not provided
        var statusId = dto.ResponseStatusId
            ?? await _intakeFormResponseRepository.GetStatusIdByNameAsync("Submitted", ct);

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

        await _intakeFormResponseRepository.AddAsync(response, ct);
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

    public async Task<List<IntakeFormResponseDto>> GetResponsesByPatientIdAsync(Guid patientId, CancellationToken ct = default)
    {
        var responses = await _intakeFormResponseRepository.GetResponsesByPatientIdAsync(patientId, ct);
        return responses.Select(entity => new IntakeFormResponseDto
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
        }).ToList();
    }
}
