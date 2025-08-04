using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeFormFieldResponses")]
public class IntakeFormFieldResponse : BaseAuditableEntity
{
    [Required]
    public Guid ResponseId { get; set; }

    [ForeignKey(nameof(ResponseId))]
    public IntakeFormResponse Response { get; set; } = default!;

    [Required]
    public Guid FieldId { get; set; }

    [ForeignKey(nameof(FieldId))]
    public IntakeFormField Field { get; set; } = default!;

    [MaxLength(2000)]
    public string? Value { get; set; }
}
