namespace Salubrity.Application.DTOs.Rbac
{
    public class CreateRolePermissionGroupDto
    {
        public Guid RoleId { get; set; }
        public Guid PermissionGroupId { get; set; }
    }
}
