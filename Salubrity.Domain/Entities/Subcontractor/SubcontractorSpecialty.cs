using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthcareServices;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Subcontractor;

[Table("SubcontractorSpecialties")]
public class SubcontractorSpecialty : BaseAuditableEntity
{
    [ForeignKey("Subcontractor")]
    public Guid SubcontractorId { get; set; }
    public virtual Salubrity.Domain.Entities.Subcontractor.Subcontractor Subcontractor { get; set; } = default!;

    [ForeignKey("Service")]
    public Guid ServiceId { get; set; }
    public virtual Service Service { get; set; } = default!;
}
