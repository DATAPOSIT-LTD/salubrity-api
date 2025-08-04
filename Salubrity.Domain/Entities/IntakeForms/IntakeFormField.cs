using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Salubrity.Domain.Entities.Lookup;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeFormFields")]
public class IntakeFormField : BaseAuditableEntity
{
    [Required]
    public Guid SectionId { get; set; }

    [ForeignKey(nameof(SectionId))]
    public IntakeFormSection Section { get; set; } = default!;

    [Required]
    [MaxLength(150)]
    public string Label { get; set; } = default!;

    [MaxLength(150)]
    public string? Placeholder { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public Guid FieldTypeId { get; set; }

    [ForeignKey(nameof(FieldTypeId))]
    public FieldType FieldType { get; set; } = default!;

    public bool IsRequired { get; set; }

    public int Order { get; set; }

    public ICollection<IntakeFormFieldOption> Options { get; set; } = new List<IntakeFormFieldOption>();
}
