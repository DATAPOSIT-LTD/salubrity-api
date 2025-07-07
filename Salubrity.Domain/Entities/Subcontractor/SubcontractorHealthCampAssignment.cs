using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthCamps;
using Salubrity.Domain.Entities.HealthcareServices;
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

    public string BoothLabel { get; set; } = default!;
    public string? RoomNumber { get; set; }

    [ForeignKey("AssignmentStatus")]
    public Guid AssignmentStatusId { get; set; }
    public virtual SubcontractorHealthCampAssignmentStatus AssignmentStatus { get; set; } = default!;

}
