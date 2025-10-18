using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthAssesment;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Join;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Organizations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthCamps;

[Table("HealthCamps")]
public class HealthCamp : BaseAuditableEntity
{
    [Required]
    public string Name { get; set; } = default!;

    public virtual ICollection<HealthCampPackage> HealthCampPackages { get; set; } = [];

    public string? Description { get; set; }
    public string? Location { get; set; }

    [Column(TypeName = "date")]
    public DateTime StartDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime? EndDate { get; set; }

    public TimeSpan? StartTime { get; set; }
    public bool IsActive { get; set; } = true;

    [ForeignKey(nameof(Organization))]
    public Guid OrganizationId { get; set; }
    public virtual Organization Organization { get; set; } = default!;

    public int? ExpectedParticipants { get; set; }
    public DateTime? CloseDate { get; set; }
    public DateTime? LaunchedAt { get; set; }
    public bool IsLaunched { get; set; }

    public virtual ICollection<HealthCampPackageItem> PackageItems { get; set; } = [];
    public virtual ICollection<HealthCampServiceAssignment> ServiceAssignments { get; set; } = [];
    public virtual ICollection<HealthCampParticipant> Participants { get; set; } = [];

    public Guid? HealthCampStatusId { get; set; }
    public virtual HealthCampStatus? HealthCampStatus { get; set; }

    public string? ParticipantPosterJti { get; set; }
    public string? SubcontractorPosterJti { get; set; }
    public DateTimeOffset? PosterTokensExpireAt { get; set; }

    public virtual ICollection<HealthCampTempCredential> TempCredentials { get; set; } = new List<HealthCampTempCredential>();
    public virtual ICollection<HealthAssessment> HealthAssessments { get; set; } = new List<HealthAssessment>();
}

