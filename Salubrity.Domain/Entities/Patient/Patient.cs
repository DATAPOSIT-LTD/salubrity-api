// File: Salubrity.Domain.Entities.Identity.Patient.cs

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Organizations;
using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Domain.Entities.Identity;

public class Patient : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Optional org-level link for ownership or affiliation
    public Guid? PrimaryOrganizationId { get; set; }
    public Organization? PrimaryOrganization { get; set; }

    // Placeholder for light metadata 
    public string? Notes { get; set; }
}
