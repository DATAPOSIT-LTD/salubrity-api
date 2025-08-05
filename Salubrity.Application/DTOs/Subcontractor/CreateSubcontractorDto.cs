namespace Salubrity.Application.DTOs.Subcontractor;

public class CreateSubcontractorDto
{
    public Guid UserId { get; set; }

    public Guid? IndustryId { get; set; }

    public string? LicenseNumber { get; set; }

    public string? Bio { get; set; }

    public Guid? StatusId { get; set; }

    // FK to services (specialties)
    public List<Guid>? SpecialtyIds { get; set; } = [];

    // FK to SubcontractorRole lookup table
    public List<Guid> SubcontractorRoleIds { get; set; } = [];
}
