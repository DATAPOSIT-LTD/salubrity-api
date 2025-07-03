using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeFormVersions")]
public class IntakeFormVersion : BaseAuditableEntity
{
    public Guid IntakeFormId { get; set; }

    [ForeignKey(nameof(IntakeFormId))]
    public IntakeForm IntakeForm { get; set; } = default!;

    public int VersionNumber { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<IntakeFormSection> Sections { get; set; } = new List<IntakeFormSection>();
}
