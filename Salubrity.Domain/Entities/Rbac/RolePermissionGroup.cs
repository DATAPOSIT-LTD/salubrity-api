using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.Rbac;

/// <summary>
/// Represents the link between a role and a permission group.
/// Assigns all permissions within the group to the role.
/// </summary>
public class RolePermissionGroup : BaseAuditableEntity
{
    /// <summary>
    /// Foreign key to the role.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// Foreign key to the permission group.
    /// </summary>
    public Guid PermissionGroupId { get; set; }

    /// <summary>
    /// Navigation property for the associated role.
    /// </summary>
    public Role Role { get; set; } = default!;

    /// <summary>
    /// Navigation property for the associated permission group.
    /// </summary>
    public PermissionGroup PermissionGroup { get; set; } = default!;
}
