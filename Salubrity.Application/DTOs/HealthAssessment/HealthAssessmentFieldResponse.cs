using Salubrity.Domain.Entities.HealthAssesment;
using Salubrity.Domain.Entities.IntakeForms;

public class HealthAssessmentFieldResponse
{
    public Guid Id { get; set; }
    public Guid HealthAssessmentId { get; set; }
    public Guid IntakeFormFieldId { get; set; }

    public string? Value { get; set; }
    public Guid? SelectedOptionId { get; set; }

    public HealthAssessment? HealthAssessment { get; set; }
    public IntakeFormField? Field { get; set; }
    public IntakeFormFieldOption? SelectedOption { get; set; }
}
