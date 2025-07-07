using Salubrity.Domain.Entities.Organizations;
using Salubrity.Domain.Entities.HealthcareServices;
using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.Join;

public class OrganizationPackage : BaseAuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid ServicePackageId { get; set; }
    public ServicePackage ServicePackage { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
