using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthAssesment;

[Table("HealthAssessmentRecommendations")]
public class HealthAssessmentRecommendation : BaseAuditableEntity
{
    [Required]
    public Guid HealthAssessmentId { get; set; }
    public virtual HealthAssessment HealthAssessment { get; set; } = default!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = default!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(30)]
    public string? Priority { get; set; } // Low/Medium/High or similar
}
