using System.ComponentModel.DataAnnotations;

namespace Salubrity.Application.DTOs.Organizations.Branches;

public class OrganizationBranchUpdateDto
{
    [Required]
    public string BranchName { get; set; } = null!;

    public string? Code { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }

    [EmailAddress]
    public string? ContactEmail { get; set; }

    [Phone]
    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; }
}
