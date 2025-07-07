namespace Salubrity.Application.DTOs.Subcontractor;

public class CreateSubcontractorRoleDto
{
    public string Name { get; set; } = default!; // e.g., "Doctor", "Nurse"

    public string? Description { get; set; }
}
