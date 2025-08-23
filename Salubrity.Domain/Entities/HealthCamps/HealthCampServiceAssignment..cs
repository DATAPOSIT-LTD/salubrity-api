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

    [ForeignKey("Service")]
    public Guid ServiceId { get; set; }
    public virtual Service Service { get; set; } = default!;

    [ForeignKey("Subcontractor")]
    public Guid SubcontractorId { get; set; }
    public virtual Salubrity.Domain.Entities.Subcontractor.Subcontractor Subcontractor { get; set; } = default!;

    [ForeignKey("Role")]
    public Guid? ProfessionId { get; set; }
    public virtual SubcontractorRole? Role { get; set; }
}
