using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.Subcontractor;

[Table("SubcontractorStatuses")]
public class SubcontractorStatus : BaseLookupEntity
{

    public bool IsActive { get; set; } = true;

    // Optional reverse nav
    public virtual ICollection<Subcontractor> Subcontractors { get; set; } = [];
}
