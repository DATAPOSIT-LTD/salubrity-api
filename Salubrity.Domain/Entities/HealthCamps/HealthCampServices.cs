using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthcareServices;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthCamps;

[Table("HealthCampServices")]
public class HealthCampService : BaseAuditableEntity
{
    [ForeignKey("HealthCamp")]
    public Guid HealthCampId { get; set; }
    public virtual HealthCamp HealthCamp { get; set; } = default!;

    [ForeignKey("Service")]
    public Guid ServiceId { get; set; }
    public virtual Service Service { get; set; } = default!;
}
