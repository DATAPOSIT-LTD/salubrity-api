using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Subcontractor;
using System.Collections.Generic;

namespace Salubrity.Domain.Entities.Lookup
{
    public class SubcontractorRole : BaseLookupEntity
    {
        public ICollection<SubcontractorRoleAssignment> SubcontractorAssignments { get; set; } = [];
    }
}
