using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeFormFieldOptions")]
public class IntakeFormFieldOption : BaseAuditableEntity
{
    public Guid FieldId { get; set; }

    [ForeignKey(nameof(FieldId))]
    public IntakeFormField Field { get; set; } = default!;

    public string Label { get; set; } = default!;
    public string? Value { get; set; }
    public int Order { get; set; }
}
