using System;
using System.Collections.Generic;

namespace Salubrity.Application.DTOs.Subcontractor
{
    public class AssignSubcontractorRoleDto
    {
        /// <summary>
        /// The list of role IDs to assign to the subcontractor
        /// </summary>
        public List<Guid> SubcontractorRoleIds { get; set; } = [];

        /// <summary>
        /// One of the role IDs above that should be marked as the primary role (optional)
        /// </summary>
        public Guid? PrimaryRoleId { get; set; }
    }
}
