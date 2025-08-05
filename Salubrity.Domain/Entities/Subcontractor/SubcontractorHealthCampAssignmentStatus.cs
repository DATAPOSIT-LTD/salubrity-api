using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Subcontractor;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Subcontractor;

[Table("SubcontractorHealthCampAssignmentStatuses")]
public class SubcontractorHealthCampAssignmentStatus : BaseAuditableEntity
{
	[MaxLength(100)]
	public string Name { get; set; } = default!; // e.g. "Pending", "Assigned", "In Progress", "Completed"

	[MaxLength(255)]
	public string? Description { get; set; }

	public bool IsActive { get; set; } = true;

	public virtual ICollection<SubcontractorHealthCampAssignment> Assignments { get; set; } = [];
}
