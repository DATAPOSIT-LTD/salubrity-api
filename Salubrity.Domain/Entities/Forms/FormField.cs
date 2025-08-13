// FormField.cs
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Forms;
using Salubrity.Domain.Entities.FormSections;
using Salubrity.Domain.Entities.FormsOptions;
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Domain.Entities.FormFields;

public class FormField : BaseAuditableEntity
{
    public Guid FormId { get; set; }
    public Form? Form { get; set; }

    public Guid? SectionId { get; set; }
    public FormSection? Section { get; set; }

    [Required, MaxLength(200)]
    public string Label { get; set; } = default!;

    [MaxLength(100)]
    public string? FieldType { get; set; }  // consider enum/lookup later

    public bool IsRequired { get; set; }
    public int Order { get; set; }

    public ICollection<FieldOption> Options { get; set; } = [];

    // Conditional logic
    public bool HasConditionalLogic { get; set; } = false;

    [MaxLength(50)]
    public string? ConditionalLogicType { get; set; } // "Show", "Hide", etc.

    public Guid? TriggerFieldId { get; set; }
    public FormField? TriggerField { get; set; }

    public Guid? TriggerValueOptionId { get; set; }
    public FieldOption? TriggerValueOption { get; set; }

    // Validation
    [MaxLength(100)]
    public string? ValidationType { get; set; }        // "Range", "Regex"
    [MaxLength(500)]
    public string? ValidationPattern { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    [MaxLength(500)]
    public string? CustomErrorMessage { get; set; }

    // Layout
    [MaxLength(50)]
    public string? LayoutPosition { get; set; }  // "Left", "Right", "FullWidth"
}
