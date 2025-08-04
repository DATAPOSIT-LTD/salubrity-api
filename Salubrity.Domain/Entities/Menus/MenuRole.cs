using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Rbac;
using System;

namespace Salubrity.Domain.Entities.Menus
{
    public class MenuRole : BaseAuditableEntity
    {
        public Guid MenuId { get; set; }
        public Menu Menu { get; set; } = default!;

        public Guid RoleId { get; set; }
        public Role Role { get; set; } = default!;

        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
