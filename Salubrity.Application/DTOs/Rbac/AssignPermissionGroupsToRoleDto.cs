namespace Salubrity.Application.DTOs.Rbac
{
    public class AssignPermissionGroupsToRoleDto
    {
        public Guid RoleId { get; set; }
        public List<Guid> PermissionGroupIds { get; set; } = [];
    }
}
