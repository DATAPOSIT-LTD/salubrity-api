#nullable enable
using Salubrity.Application.Common.Interfaces.Repositories;
using Salubrity.Application.DTOs.Forms.IntakeFormResponses;
using Salubrity.Application.DTOs.HealthCamps;
using Salubrity.Application.DTOs.IntakeForms;
using Salubrity.Application.Interfaces.Repositories.Camps;
using Salubrity.Application.Interfaces.Repositories.HealthCamps;
using Salubrity.Application.Interfaces.Repositories.HealthcareServices;
using Salubrity.Application.Interfaces.Repositories.IntakeForms;
using Salubrity.Application.Interfaces.Services.IntakeForms;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Shared.Exceptions;

namespace Salubrity.Application.Services.Forms;

public sealed class IntakeFormResponseService : IIntakeFormResponseService
{
    private readonly IIntakeFormResponseRepository _intakeFormResponseRepository;
    private readonly IHealthCampParticipantRepository _participantRepository;
    private readonly ICampQueueRepository _stationCheckInRepository;
    private readonly IServiceRepository _serviceRepository;
    private readonly IServiceCategoryRepository _serviceCategoryRepository;
    private readonly IServiceSubcategoryRepository _serviceSubcategoryRepository;
    private readonly IHealthCampServiceAssignmentRepository _assignmentRepository;

    public IntakeFormResponseService(
        IIntakeFormResponseRepository intakeFormResponseRepository,
        IHealthCampParticipantRepository participantRepository,
        ICampQueueRepository campQueueRepository,
        IHealthCampServiceAssignmentRepository assignmentRepository,
        IServiceRepository serviceRepository,
        IServiceCategoryRepository serviceCategoryRepository,
        IServiceSubcategoryRepository serviceSubcategoryRepository
    )
    {
        _intakeFormResponseRepository = intakeFormResponseRepository;
        _participantRepository = participantRepository;
        _stationCheckInRepository = campQueueRepository;
        _assignmentRepository = assignmentRepository;
        _serviceRepository = serviceRepository;
        _serviceCategoryRepository = serviceCategoryRepository;
        _serviceSubcategoryRepository = serviceSubcategoryRepository;
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
        var versionExists = await _intakeFormResponseRepository.IntakeFormVersionExistsAsync(dto.IntakeFormVersionId, ct);
        if (!versionExists)
            throw new NotFoundException("Form version not found.");

        var validFieldIds = await _intakeFormResponseRepository.GetFieldIdsForVersionAsync(dto.IntakeFormVersionId, ct);
        var invalidField = dto.FieldResponses.FirstOrDefault(f => !validFieldIds.Contains(f.FieldId));
        if (invalidField is not null)
            throw new ValidationException([$"Field {invalidField.FieldId} does not belong to form version {dto.IntakeFormVersionId}."]);

        var patientId = await _participantRepository.GetPatientIdByParticipantIdAsync(dto.PatientId, ct);
        if (patientId is null)
            throw new NotFoundException($"Patient not found for participant {dto.PatientId}.");

        Guid submittedServiceId;
        PackageItemType submittedServiceType;
        Guid resolvedServiceId;

        if (dto.HealthCampServiceAssignmentId.HasValue)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(dto.HealthCampServiceAssignmentId.Value, ct);
            if (assignment is null)
                throw new NotFoundException($"Assignment not found: {dto.HealthCampServiceAssignmentId}");

            submittedServiceId = assignment.AssignmentId;
            submittedServiceType = assignment.AssignmentType;

            switch (assignment.AssignmentType)
            {
                case PackageItemType.Service:
                    resolvedServiceId = assignment.AssignmentId;
                    break;

                case PackageItemType.ServiceCategory:
                    var category = await _serviceCategoryRepository.GetByIdAsync(assignment.AssignmentId);
                    if (category?.ServiceId == null)
                        throw new ValidationException(["ServiceCategory is not linked to a root Service."]);
                    resolvedServiceId = category.ServiceId;
                    break;

                case PackageItemType.ServiceSubcategory:
                    var subcategory = await _serviceSubcategoryRepository.GetByIdAsync(assignment.AssignmentId);
                    if (subcategory?.ServiceCategory?.ServiceId == null)
                        throw new ValidationException(["Subcategory is not linked to a root Service via its category."]);
                    resolvedServiceId = subcategory.ServiceCategory.ServiceId;
                    break;

                default:
                    throw new ValidationException(["Unknown assignment type for response submission."]);
            }
        }
        else if (dto.ServiceId.HasValue)
        {
            submittedServiceId = dto.ServiceId.Value;
            submittedServiceType = PackageItemType.Service;
            resolvedServiceId = dto.ServiceId.Value;
        }
        else
        {
            throw new ValidationException(["Either HealthCampServiceAssignmentId or ServiceId must be provided."]);
        }

        var statusId = dto.ResponseStatusId
            ?? await _intakeFormResponseRepository.GetStatusIdByNameAsync("Submitted", ct);

        var responseId = Guid.NewGuid();
        var response = new IntakeFormResponse
        {
            Id = responseId,
            IntakeFormVersionId = dto.IntakeFormVersionId,
            SubmittedByUserId = submittedByUserId,
            PatientId = patientId.Value,
            SubmittedServiceId = submittedServiceId,
            SubmittedServiceType = submittedServiceType,
            ResolvedServiceId = resolvedServiceId,
            ResponseStatusId = statusId,
            FieldResponses = dto.FieldResponses.Select(f => new IntakeFormFieldResponse
            {
                Id = Guid.NewGuid(),
                ResponseId = responseId,
                FieldId = f.FieldId,
                Value = f.Value
            }).ToList()
        };

        await _intakeFormResponseRepository.AddAsync(response, ct);

        HealthCampStationCheckIn? checkIn = null;

        if (dto.StationCheckInId is Guid explicitCheckInId)
        {
            checkIn = await _stationCheckInRepository.GetByIdAsync(explicitCheckInId, ct);
            if (checkIn is null)
                throw new NotFoundException($"Station check-in not found: {explicitCheckInId}");
        }
        else
        {
            checkIn = await _stationCheckInRepository.GetActiveForParticipantAsync(
                dto.PatientId,
                dto.HealthCampServiceAssignmentId,
                ct);
        }

        if (checkIn is not null)
        {
            if (checkIn.Status is CampQueueStatus.Canceled or CampQueueStatus.Completed)
                throw new ValidationException([$"Cannot complete a check-in in '{checkIn.Status}' status."]);

            var now = DateTimeOffset.UtcNow;
            checkIn.Status = CampQueueStatus.Completed;
            checkIn.FinishedAt = now;
            checkIn.StartedAt ??= now;

            await _stationCheckInRepository.UpdateAsync(checkIn, ct);
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
            IntakeFormId = entity.Version?.IntakeFormId ?? Guid.Empty, // ← include this!
            IntakeFormVersionId = entity.IntakeFormVersionId,
            SubmittedByUserId = entity.SubmittedByUserId,
            PatientId = entity.PatientId,
            SubmittedServiceId = entity.SubmittedServiceId,
            SubmittedServiceType = entity.SubmittedServiceType,
            ResolvedServiceId = entity.ResolvedServiceId,
            ResponseStatusId = entity.ResponseStatusId,
            FieldResponses = entity.FieldResponses
                .OrderBy(f => f.CreatedAt)
                .Select(field => new IntakeFormFieldResponseDto
                {
                    Id = field.Id,
                    ResponseId = field.ResponseId,
                    FieldId = field.FieldId,
                    Value = field.Value
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
