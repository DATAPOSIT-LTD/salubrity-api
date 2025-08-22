// File: Salubrity.Application/DTOs/IntakeForms/IntakeFormResponseDto.cs

namespace Salubrity.Application.DTOs.IntakeForms;



public sealed class IntakeFormFieldResponseDto
{
    public Guid Id { get; set; }

    public Guid ResponseId { get; set; }

    public Guid FieldId { get; set; }

    public string? Value { get; set; }
}


public class CreateIntakeFormResponseDto
{
    public Guid IntakeFormVersionId { get; set; }
    public Guid ServiceId { get; set; }
    public List<IntakeFormFieldResponseDto> FieldResponses { get; set; } = [];
}


