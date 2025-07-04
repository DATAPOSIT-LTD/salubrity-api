using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Organizations;

namespace Salubrity.Domain.Entities.Join;
public class OrganizationInsuranceProvider : BaseAuditableEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid InsuranceProviderId { get; set; }
    public InsuranceProvider InsuranceProvider { get; set; } = null!;
}
