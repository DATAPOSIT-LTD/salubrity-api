namespace Salubrity.Application.DTOs.Organizations.Branches;

public class OrganizationBranchResponseDto
{
    public Guid Id { get; set; }
    public string BranchName { get; set; } = null!;

    public string? Code { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }

    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }

    public bool IsActive { get; set; }
}
