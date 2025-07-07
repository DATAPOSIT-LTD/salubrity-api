using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Organizations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Subcontractor;

[Table("Subcontractors")]
public class Subcontractor : BaseAuditableEntity
{
    [ForeignKey("User")]
    public Guid UserId { get; set; }
    public virtual Salubrity.Domain.Entities.Identity.User User { get; set; } = default!; 

    [ForeignKey("Industry")]
    public Guid IndustryId { get; set; }
    public virtual Industry Industry { get; set; } = default!;

    [MaxLength(100)]
    public string? LicenseNumber { get; set; } = default!;

    public string? Bio { get; set; }

    // FK to status table instead of enum
    [ForeignKey("Status")]
    public Guid StatusId { get; set; }
    public virtual SubcontractorStatus Status { get; set; } = default!;

    // Navigation Properties
    public virtual ICollection<SubcontractorSpecialty> Specialties { get; set; } = new List<SubcontractorSpecialty>();
    public virtual ICollection<SubcontractorRole> Roles { get; set; } = new List<SubcontractorRole>();
    public virtual ICollection<SubcontractorHealthCampAssignment> CampAssignments { get; set; } = new List<SubcontractorHealthCampAssignment>();
}
