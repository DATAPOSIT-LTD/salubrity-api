namespace Salubrity.Application.DTOs.Subcontractor;

public class SubcontractorRoleDto
{
    public Guid RoleId { get; set; }
    public Guid SubcontractorId { get; set; }
    public string RoleName { get; set; } = default!;
    public string? Description { get; set; }
}

