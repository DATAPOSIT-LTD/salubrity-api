// File: Salubrity.Domain.Entities.HealthCamps.HealthCampParticipant.cs

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthAssesment;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Lookup;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Join;

public class HealthCampParticipant : BaseAuditableEntity
{
    public Guid HealthCampId { get; set; }
    public HealthCamp HealthCamp { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? PatientId { get; set; }
    public Patient? Patient { get; set; }

    public bool IsEmployee { get; set; } = false;

    public string? Notes { get; set; }

    [ForeignKey("BillingStatus")]
    public Guid? BillingStatusId { get; set; }
    public virtual BillingStatus? BillingStatus { get; set; }

    public DateTime? ParticipatedAt { get; set; }
    public string? TempPasswordHash { get; set; }
    public DateTimeOffset? TempPasswordExpiresAt { get; set; }
    public ICollection<HealthAssessment> HealthAssessments { get; set; } = [];

    public Guid? HealthCampPackageId { get; set; }
    public HealthCampPackage? HealthCampPackage { get; set; }
}
