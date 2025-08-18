using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Subcontractor;

[Table("SubcontractorHealthCampAssignments")]
public class SubcontractorHealthCampAssignment : BaseAuditableEntity
{
    [ForeignKey("HealthCamp")]
    public Guid HealthCampId { get; set; }
    public virtual HealthCamp HealthCamp { get; set; } = default!;

    [ForeignKey("Subcontractor")]
    public Guid SubcontractorId { get; set; }
    public virtual Subcontractor Subcontractor { get; set; } = default!;

    [ForeignKey("ServiceCategory")]
    public Guid? ServiceCategoryId { get; set; }
    public virtual ServiceCategory? ServiceCategory { get; set; }

    [ForeignKey("ServiceSubcategory")]
    public Guid? ServiceSubcategoryId { get; set; }
    public virtual ServiceSubcategory? ServiceSubcategory { get; set; }

    [Required, MaxLength(100)]
    public string BoothLabel { get; set; } = default!;

    [MaxLength(50)]
    public string? RoomNumber { get; set; }

    [ForeignKey("AssignmentStatus")]
    public Guid AssignmentStatusId { get; set; }
    public virtual SubcontractorHealthCampAssignmentStatus AssignmentStatus { get; set; } = default!;

    [Required]
    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [MaxLength(255)]
    public string? Notes { get; set; }

    // Useful in future if you need to mark a fallback provider or secondary booth
    public bool IsPrimaryAssignment { get; set; } = true;

    public string? TempPasswordHash { get; set; }
    public DateTimeOffset? TempPasswordExpiresAt { get; set; }
}
