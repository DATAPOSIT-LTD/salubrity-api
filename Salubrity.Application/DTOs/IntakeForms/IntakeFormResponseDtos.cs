using Salubrity.Application.DTOs.IntakeForms;

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

    public Guid? ServiceId { get; set; }

    public Guid? ResponseStatusId { get; set; }

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

    public Guid IntakeFormId { get; set; } // 

    public Guid IntakeFormVersionId { get; set; }

    public Guid SubmittedByUserId { get; set; }

    public Guid PatientId { get; set; }

    public Guid? ServiceId { get; set; }

    public Guid ResponseStatusId { get; set; }

    public List<IntakeFormFieldResponseDto> FieldResponses { get; set; } = [];
}
