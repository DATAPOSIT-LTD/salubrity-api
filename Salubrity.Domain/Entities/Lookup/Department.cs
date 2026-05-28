using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Identity;
using System.Collections.Generic;

namespace Salubrity.Domain.Entities.Lookup
{
    public class Department : BaseLookupEntity
    {
        /// <summary>Nullable: when set, this department is scoped to a specific organization.</summary>
        public Guid? OrganizationId { get; set; }

        public ICollection<Employee> Employees { get; set; } = [];
    }
}
