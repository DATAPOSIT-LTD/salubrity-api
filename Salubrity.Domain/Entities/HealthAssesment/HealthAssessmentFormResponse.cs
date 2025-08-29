using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthAssesment;
using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Domain.Entities.Lookup;

public class HealthAssessmentFormResponse : BaseAuditableEntity
{
    // public Guid HealthAssessmentId { get; set; }
    // public HealthAssessment HealthAssessment { get; set; } = default!;

    public Guid FormTypeId { get; set; }
    public HealthAssessmentFormType FormType { get; set; } = default!;

    public Guid? IntakeFormVersionId { get; set; }
    public IntakeFormVersion? IntakeFormVersion { get; set; }

    public ICollection<HealthAssessmentDynamicFieldResponse> Responses { get; set; } = [];



}

