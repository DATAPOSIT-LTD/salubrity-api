using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.DTOs.IntakeForms;

public sealed class CreateIntakeFormFieldResponseDto
{
    public Guid FieldId { get; set; }

    public string? Value { get; set; }
}

public sealed class CreateIntakeFormResponseDto
{
    public Guid IntakeFormId { get; set; }
    public Guid IntakeFormVersionId { get; set; }

    public Guid PatientId { get; set; }

    // New service logic
    public Guid? ServiceId { get; set; }                     // fallback if no assignment
    public Guid? HealthCampServiceAssignmentId { get; set; } // strongly preferred
    public Guid? StationCheckInId { get; set; }

    public Guid? ResponseStatusId { get; set; }

    public Guid ParticipantId { get; set; }

    public List<CreateIntakeFormFieldResponseDto> FieldResponses { get; set; } = [];
}


public sealed class IntakeFormFieldResponseDto
{
    public Guid Id { get; set; }

    public Guid ResponseId { get; set; }

    public Guid FieldId { get; set; }

    public string? Value { get; set; }
}

public sealed class IntakeFormResponseDto
{
    public Guid Id { get; set; }

    public Guid IntakeFormId { get; set; }

    public Guid IntakeFormVersionId { get; set; }

    public Guid SubmittedByUserId { get; set; }

    public Guid PatientId { get; set; }

    // 🔥 NEW SERVICE TRACKING FIELDS
    public Guid? SubmittedServiceId { get; set; }           // Raw ID from user/assignment
    public PackageItemType? SubmittedServiceType { get; set; } // Enum: Service / Category / Subcategory
    public Guid? ResolvedServiceId { get; set; }            // Final FK to Service

    public Guid ResponseStatusId { get; set; }

    public List<IntakeFormFieldResponseDto> FieldResponses { get; set; } = [];
}
