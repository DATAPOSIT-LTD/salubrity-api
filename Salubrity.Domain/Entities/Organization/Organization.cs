using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;
using Salubrity.Domain.Entities.Join;
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Domain.Entities.Organizations;

public class Organization : BaseAuditableEntity
{
    [Required]
    public string BusinessName { get; set; } = null!;

    [EmailAddress]
    public string Email { get; set; } = null!;

    [Phone]
    public string Phone { get; set; } = null!;

    public string? Location { get; set; }

    public string? ClientLogoPath { get; set; }

    public Guid? ContactPersonId { get; set; }

    public Guid? StatusId { get; set; }
    public OrganizationStatus? Status { get; set; }

    public ICollection<OrganizationInsuranceProvider> InsuranceProviders { get; set; } = [];
    public ICollection<OrganizationPackage> Packages { get; set; } = [];

}
