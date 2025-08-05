using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.Rbac
{
    /// <summary>
    /// Represents a single, atomic permission (e.g., "user:create", "form:submit") used to control access.
    /// </summary>
    public class Permission : BaseAuditableEntity
    {
        /// <summary>
        /// Unique code representing the permission (e.g., "appointment:read", "user:update").
        /// </summary>
        public string Code { get; set; } = default!;

        /// <summary>
        /// Optional description of what this permission allows.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Permission groups that include this permission.
        /// </summary>
        public ICollection<PermissionGroupPermission> GroupPermissions { get; set; } = [];

        /// <summary>
        /// Generate a user-friendly name from the permission code.
        /// </summary>
        public string GetPermissionName()
        {
            if (string.IsNullOrEmpty(Code))
                return "Unknown Permission";

            // Example: "user:create" -> "Create User"
            var parts = Code.Split(':');
            if (parts.Length == 2)
            {
                var action = parts[1].ToLowerInvariant(); // "create"
                var resource = parts[0].ToLowerInvariant(); // "user"

                // Capitalize the first letter of each word
                return $"{char.ToUpperInvariant(resource[0]) + resource.Substring(1)} {char.ToUpperInvariant(action[0]) + action.Substring(1)}";
            }

            return Code; // Fallback if the format is not as expected
        }
    }
}
