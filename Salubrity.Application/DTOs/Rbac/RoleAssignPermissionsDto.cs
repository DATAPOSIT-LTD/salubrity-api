namespace Salubrity.Application.DTOs.Rbac;

public class AssignPermissionsToRoleDto
{
    public Guid RoleId { get; set; }
    public List<Guid> PermissionIds { get; set; } = new();
}
