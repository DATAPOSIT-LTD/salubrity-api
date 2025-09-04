using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.IntakeForms;

public class HealthAssessmentDynamicFieldResponse : BaseAuditableEntity
{
    public Guid FormResponseId { get; set; }
    public HealthAssessmentFormResponse FormResponse { get; set; } = default!;

    public Guid FieldId { get; set; }
    public Guid? SectionId { get; set; }
    public IntakeFormField Field { get; set; } = default!;

    public string? Value { get; set; }
    public Guid? SelectedOptionId { get; set; }
}
