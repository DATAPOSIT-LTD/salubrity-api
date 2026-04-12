namespace Salubrity.Application.DTOs.Forms.IntakeFormResponses;

public class PatchIntakeFormResponseDto
{
    public Guid ResponseId { get; set; }

    // Optional status change (Draft → Submitted etc.)
    public Guid? ResponseStatusId { get; set; }

    public List<PatchIntakeFormFieldResponseDto> FieldResponses { get; set; } = new();
}
