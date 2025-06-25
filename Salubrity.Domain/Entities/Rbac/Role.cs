using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.Rbac;

/// <summary>
/// Represents a system role assigned to users, which defines access and responsibility scope via permission groups.
/// </summary>
public class Role : BaseAuditableEntity
{
    /// <summary>
    /// Display name of the role (e.g., "Admin", "Nurse", "Finance Manager").
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Optional description for the role.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates whether the role is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Icon CSS class for UI display (e.g., FontAwesome).
    /// </summary>
    public string? IconClass { get; set; }  // nullable string

    /// <summary>
    /// If assigned, this role is scoped to a specific organization.
    /// If null, the role is considered global.
    /// </summary>
    public Guid? OrganizationId { get; set; }

    /// <summary>
    /// Whether the role is globally available across organizations.
    /// </summary>
    public bool IsGlobal { get; set; } = false;

    /// <summary>
    /// Integer order for sorting menu items.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Users assigned to this role.
    /// </summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    /// <summary>
    /// Permission groups associated with this role.
    /// </summary>
    public ICollection<RolePermissionGroup> RolePermissionGroups { get; set; } = new List<RolePermissionGroup>();
}
