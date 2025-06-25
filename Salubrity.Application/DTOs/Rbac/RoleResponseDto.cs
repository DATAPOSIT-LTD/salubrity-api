namespace Salubrity.Application.DTOs.Rbac
{
    /// <summary>
    /// Represents a full role for response/view purposes.
    /// </summary>
    public class RoleDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsGlobal { get; set; }
        public Guid? OrganizationId { get; set; }
     
    }

    /// <summary>
    /// DTO for creating a new role.
    /// </summary>
    public class CreateRoleDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsGlobal { get; set; } = false;
        public Guid? OrganizationId { get; set; }

    }

    /// <summary>
    /// DTO for updating an existing role.
    /// </summary>
    public class UpdateRoleDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsGlobal { get; set; }
        public Guid? OrganizationId { get; set; }
        
    }
}
