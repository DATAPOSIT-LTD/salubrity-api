namespace Salubrity.Application.DTOs.Rbac;

public class UpdatePermissionGroupDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
