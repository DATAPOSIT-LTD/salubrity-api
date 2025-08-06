using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Subcontractor;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Subcontractor;

[Table("SubcontractorHealthCampAssignmentStatuses")]
public class SubcontractorHealthCampAssignmentStatus : BaseLookupEntity
{

	public bool IsActive { get; set; } = true;

	public virtual ICollection<SubcontractorHealthCampAssignment> Assignments { get; set; } = [];
}
