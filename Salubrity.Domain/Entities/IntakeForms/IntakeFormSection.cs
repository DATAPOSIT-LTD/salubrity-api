using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeFormSections")]
public class IntakeFormSection : BaseAuditableEntity
{
    public Guid IntakeFormVersionId { get; set; }

    [ForeignKey(nameof(IntakeFormVersionId))]
    public IntakeFormVersion Version { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public int Order { get; set; }

    public ICollection<IntakeFormField> Fields { get; set; } = new List<IntakeFormField>();
}
