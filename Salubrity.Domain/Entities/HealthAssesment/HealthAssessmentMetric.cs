using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthAssesment;

[Table("HealthAssessmentMetrics")]
public class HealthAssessmentMetric : BaseAuditableEntity
{
    [Required]
    public Guid HealthAssessmentId { get; set; }
    public virtual HealthAssessment HealthAssessment { get; set; } = default!;

    [Required, MaxLength(150)]
    public string Name { get; set; } = default!;

    public decimal? Value { get; set; }

    [MaxLength(100)]
    public string? ReferenceRange { get; set; }

    // Use your lookup table for status
    public Guid? HealthMetricStatusId { get; set; }
    public virtual HealthMetricStatus? Status { get; set; }
}
