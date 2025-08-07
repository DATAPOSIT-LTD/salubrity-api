namespace Salubrity.Application.DTOs.Subcontractor;

public class SubcontractorDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public string? IndustryName { get; set; }
    public string? StatusName { get; set; }

    public string? LicenseNumber { get; set; }
    public string? Bio { get; set; }

    public List<SubcontractorSpecialtyDto> Specialties { get; set; } = [];
    public List<SubcontractorRoleDto> Roles { get; set; } = [];

    public int CampAssignmentCount { get; set; }
}
