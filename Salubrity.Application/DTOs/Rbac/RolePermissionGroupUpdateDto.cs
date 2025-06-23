namespace Salubrity.Application.DTOs.Rbac
{
    public class UpdateRolePermissionGroupDto
    {
        public Guid RoleId { get; set; }
        public Guid PermissionGroupId { get; set; }
    }
}
