using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.Rbac;

/// <summary>
/// Represents the link between a permission group and an individual permission.
/// Enables grouping of permissions under logical access domains.
/// </summary>
public class PermissionGroupPermission : BaseAuditableEntity
{
    /// <summary>
    /// Foreign key to the permission group.
    /// </summary>
    public Guid PermissionGroupId { get; set; }

    /// <summary>
    /// Foreign key to the permission.
    /// </summary>
    public Guid PermissionId { get; set; }

    /// <summary>
    /// Navigation property for the permission group.
    /// </summary>
    public PermissionGroup PermissionGroup { get; set; } = default!;

    /// <summary>
    /// Navigation property for the permission.
    /// </summary>
    public Permission Permission { get; set; } = default!;
}
