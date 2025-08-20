using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Rbac;
using System;
using System.Collections.Generic;

namespace Salubrity.Domain.Entities.Menus
{
    public class Menu : BaseAuditableEntity
    {
        public string Label { get; set; } = default!;
        public string Path { get; set; } = default!;
        public string? Icon { get; set; }
        public int Order { get; set; }

        public Guid? ParentId { get; set; }
        public Menu? Parent { get; set; }
        public ICollection<Menu> Children { get; set; } = [];

        public Guid? RequiredPermissionId { get; set; }
        public Permission? RequiredPermission { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<MenuRole> MenuRoles { get; set; } = [];
    }
}
