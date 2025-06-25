using System;
using System.Collections.Generic;

namespace Salubrity.Application.DTOs.Menus
{
    public class MenuRoleCreateDto
    {
        public Guid MenuId { get; set; }
        public List<Guid> RoleIds { get; set; } = new();
    }
}
