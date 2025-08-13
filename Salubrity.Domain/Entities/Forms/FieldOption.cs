// FieldOption.cs
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.FormFields;
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Domain.Entities.FormsOptions;

public class FieldOption : BaseAuditableEntity
{
    public Guid FormFieldId { get; set; }
    public FormField? FormField { get; set; }

    [Required, MaxLength(200)]
    public string Value { get; set; } = default!;

    [Required, MaxLength(200)]
    public string DisplayText { get; set; } = default!;

    public int Order { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
