using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthAssesment;

[Table("HealthMetricConfigs")]
public class HealthMetricConfig : BaseAuditableEntity
{
    [Required, MaxLength(150)]
    public string Name { get; set; } = default!;

    [MaxLength(255)]
    public string? Description { get; set; }

    public Guid? ValueTypeId { get; set; }
    public virtual MetricValueType? ValueType { get; set; }

    public Guid? ScoreMethodId { get; set; }
    public virtual MetricScoreMethod? ScoreMethod { get; set; }

    public decimal? MinAcceptable { get; set; }
    public decimal? MaxAcceptable { get; set; }

    [MaxLength(2000)]
    public string? InterpretationFormula { get; set; }

    public virtual ICollection<HealthMetricThreshold> Thresholds { get; set; } = new List<HealthMetricThreshold>();
}
