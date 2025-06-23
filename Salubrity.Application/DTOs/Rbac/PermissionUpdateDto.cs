namespace Salubrity.Application.DTOs.Rbac;

public class PermissionUpdateDto
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
}
