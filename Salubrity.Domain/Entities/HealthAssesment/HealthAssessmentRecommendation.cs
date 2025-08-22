using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthAssesment;

[Table("HealthAssessmentRecommendations")]
public class HealthAssessmentRecommendation : BaseAuditableEntity
{
    [Required] public Guid HealthAssessmentId { get; set; }
    public virtual HealthAssessment HealthAssessment { get; set; } = default!;

    [Required, MaxLength(150)] public string Name { get; set; } = default!;

    [MaxLength(150)] public string? Title { get; set; } // e.g. “Improve Cardiovascular Health”
    [MaxLength(1000)] public string? Description { get; set; } // e.g. “Consider regular walking, reduce red meat...”

    public int? Priority { get; set; } // For sorting or weighting
    public decimal? Value { get; set; }

    public Guid? MetricConfigId { get; set; }
    public virtual HealthMetricConfig? Config { get; set; }

    public Guid? HealthMetricStatusId { get; set; }
    public virtual HealthMetricStatus? Status { get; set; }
}
