namespace Salubrity.Application.DTOs.Rbac
{
    public class RolePermissionGroupDto
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; } 
        public Guid PermissionGroupId { get; set; } 
    }
}
