using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeFormFieldOptions")]
public class IntakeFormFieldOption : BaseAuditableEntity
{
    [Required]
    public Guid FieldId { get; set; }

    [ForeignKey(nameof(FieldId))]
    public IntakeFormField FormField { get; set; } = default!;

    [Required]
    [MaxLength(150)]
    public string Label { get; set; } = default!;

    [MaxLength(255)]
    public bool IsDefault { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public int Order { get; set; }

    [Required, MaxLength(200)]
    public string Value { get; set; } = default!;

    [Required, MaxLength(200)]
    public string DisplayText { get; set; } = default!;

}




