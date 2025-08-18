using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Join;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthAssesment;

[Table("HealthAssessments")]
public class HealthAssessment : BaseAuditableEntity
{
    [Required]
    public Guid HealthCampId { get; set; }
    public virtual HealthCamp HealthCamp { get; set; } = default!;

    [Required]
    public Guid ParticipantId { get; set; }
    public virtual HealthCampParticipant Participant { get; set; } = default!;

    public int OverallScore { get; set; }

    public Guid? ReviewedById { get; set; }
    public virtual Employee? ReviewedBy { get; set; }

    public virtual ICollection<HealthAssessmentMetric> Metrics { get; set; } = [];
    public virtual ICollection<HealthAssessmentRecommendation> Recommendations { get; set; } = [];
}
