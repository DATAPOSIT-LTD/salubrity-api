using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

public enum FieldType
{
    Text,
    Number,
    Date,
    Dropdown,
    Radio,
    Checkbox,
    CheckboxGroup,
    File,
    Textarea,
    Tel
}

[Table("IntakeFormFields")]
public class IntakeFormField : BaseAuditableEntity
{
    public Guid SectionId { get; set; }

    [ForeignKey(nameof(SectionId))]
    public IntakeFormSection Section { get; set; } = default!;

    public string Label { get; set; } = default!;
    public string? Placeholder { get; set; }
    public string? Description { get; set; }

    public FieldType FieldType { get; set; }

    public bool IsRequired { get; set; }

    public int Order { get; set; }

    public ICollection<IntakeFormFieldOption> Options { get; set; } = new List<IntakeFormFieldOption>();
}
