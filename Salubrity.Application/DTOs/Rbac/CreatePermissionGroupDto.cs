namespace Salubrity.Application.DTOs.Rbac;

public class CreatePermissionGroupDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
}
