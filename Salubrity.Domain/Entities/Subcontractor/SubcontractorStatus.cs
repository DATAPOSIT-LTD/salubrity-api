using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Subcontractor;

[Table("SubcontractorStatuses")]
public class SubcontractorStatus : BaseAuditableEntity
{
    [MaxLength(100)]
    public string Name { get; set; } = default!; // e.g., "Active", "Inactive", "Suspended"

    [MaxLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    // Optional reverse nav
    public virtual ICollection<Subcontractor> Subcontractors { get; set; } = new List<Subcontractor>();
}
