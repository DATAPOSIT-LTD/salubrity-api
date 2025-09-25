using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Entities.Lookup;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthCamps;


[Table("HealthCampServiceAssignments")]
public class HealthCampServiceAssignment : BaseAuditableEntity
{
    [ForeignKey("HealthCamp")]
    public Guid HealthCampId { get; set; }
    public virtual HealthCamp HealthCamp { get; set; } = default!;

    public Guid AssignmentId { get; set; } // could be service, category, or subcategory

    public PackageItemType AssignmentType { get; set; }

    [ForeignKey("Subcontractor")]
    public Guid SubcontractorId { get; set; }
    public virtual Subcontractor.Subcontractor Subcontractor { get; set; } = default!;

    [ForeignKey("Role")]
    public Guid? ProfessionId { get; set; }
    public virtual SubcontractorRole? Role { get; set; }

    // You can add helper properties (not mapped to DB) to load the target entity
    [NotMapped]
    public string? AssignmentName { get; set; } // populated manually depending on type
}

