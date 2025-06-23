using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.Rbac;

/// <summary>
/// Represents a logical grouping of permissions, such as "Form Management" or "Camp Access".
/// Used to simplify role-to-permission assignments.
/// </summary>
public class PermissionGroup : BaseAuditableEntity
{
    /// <summary>
    /// Unique name for the permission group (e.g., "Form Management").
    /// </summary>
    public string Name { get; set; } = default!;

    /// <summary>
    /// Optional description of the permission group.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Permissions associated with this group.
    /// </summary>
    public ICollection<PermissionGroupPermission> PermissionGroupPermissions { get; set; } = new List<PermissionGroupPermission>();

    /// <summary>
    /// Roles associated with this permission group.
    /// </summary>
    public ICollection<RolePermissionGroup> RolePermissionGroups { get; set; } = new List<RolePermissionGroup>();
}
