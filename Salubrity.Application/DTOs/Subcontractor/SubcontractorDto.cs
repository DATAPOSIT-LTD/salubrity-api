namespace Salubrity.Application.DTOs.Subcontractor;

public class SubcontractorDto
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public string FullName { get; set; } = default!; // From User

    public Guid? IndustryId { get; set; }
    public string? IndustryName { get; set; }

    public string? LicenseNumber { get; set; }
    public string? Bio { get; set; }

    public Guid? StatusId { get; set; }
    public string? StatusName { get; set; }

    public List<SubcontractorSpecialtyDto> Specialties { get; set; } = [];
    public List<SubcontractorRoleDto> Roles { get; set; } = [];
}


