using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Domain.Entities.Organizations;


public class OrganizationBranch : BaseAuditableEntity
{
    [Required]
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    [Required]
    public string BranchName { get; set; } = null!;

    public string? Code { get; set; }   // e.g., "EQT-005"
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }

    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; } = true;
}
