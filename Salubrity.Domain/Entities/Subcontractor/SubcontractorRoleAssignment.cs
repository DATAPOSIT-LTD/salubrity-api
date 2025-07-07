using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Subcontractor
{
    [Table("SubcontractorRoleAssignments")]
    public class SubcontractorRoleAssignment : BaseAuditableEntity
    {
        [ForeignKey("Subcontractor")]
        public Guid SubcontractorId { get; set; }
        public virtual Subcontractor Subcontractor { get; set; } = default!;

        [ForeignKey("SubcontractorRole")]
        public Guid SubcontractorRoleId { get; set; }
        public virtual SubcontractorRole SubcontractorRole { get; set; } = default!;
    }
}
