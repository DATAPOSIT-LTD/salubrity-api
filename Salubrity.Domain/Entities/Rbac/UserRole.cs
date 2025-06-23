using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.Rbac;

/// <summary>
/// Represents the assignment of a role to a specific user.
/// Supports multi-role-per-user systems.
/// </summary>
public class UserRole : BaseAuditableEntity
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The unique identifier of the role assigned to the user.
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// The timestamp when the role was assigned to the user.
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property for the associated role.
    /// </summary>
    public Role Role { get; set; } = default!;

    // Optional
    //public User User { get; set; } = default!;
}
