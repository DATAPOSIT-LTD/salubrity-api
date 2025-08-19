using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthAssesment;

[Table("HealthMetricThresholds")]
public class HealthMetricThreshold : BaseAuditableEntity
{
    [Required]
    public Guid MetricConfigId { get; set; }
    public virtual HealthMetricConfig MetricConfig { get; set; } = default!;

    public decimal MinValue { get; set; }
    public decimal MaxValue { get; set; }

    public int Score { get; set; }

    [MaxLength(150)]
    public string StatusLabel { get; set; } = default!;

    public Guid? StatusId { get; set; }
    public virtual HealthMetricStatus? Status { get; set; }
}
