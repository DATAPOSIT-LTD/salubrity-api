// IntakeForm.cs
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.IntakeForms;
using System.ComponentModel.DataAnnotations;

namespace Salubrity.Domain.Entities.IntakeForms;

public class IntakeFormField : BaseAuditableEntity
{
    public Guid FormId { get; set; }
    public IntakeForm? Form { get; set; }

    public Guid? SectionId { get; set; }
    public IntakeFormSection? Section { get; set; }

    [Required, MaxLength(200)]
    public string Label { get; set; } = default!;

    [MaxLength(100)]
    public string? FieldType { get; set; }  // consider enum/lookup later

    public bool IsRequired { get; set; }
    public int Order { get; set; }

    public ICollection<IntakeFormFieldOption> Options { get; set; } = [];

    // Conditional logic
    public bool HasConditionalLogic { get; set; } = false;

    [MaxLength(50)]
    public string? ConditionalLogicType { get; set; } // "Show", "Hide", etc.

    public Guid? TriggerFieldId { get; set; }
    public IntakeFormField? TriggerField { get; set; }

    public Guid? TriggerValueOptionId { get; set; }
    public IntakeFormFieldOption? TriggerValueOption { get; set; }

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
