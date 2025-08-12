using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Forms;
using Salubrity.Domain.Entities.FormSections;
using Salubrity.Domain.Entities.FormsOptions;

namespace Salubrity.Domain.Entities.FormFields;

public class FormField : BaseAuditableEntity
{


    public Guid FormId { get; set; }
    public Form? Form { get; set; }
    public Guid? SectionId { get; set; }
    public FormSection? Section { get; set; }
    public string? Label { get; set; }
    public string? FieldType { get; set; }
    public bool IsRequired { get; set; }
    public int Order { get; set; }
    public ICollection<FieldOption> Options { get; set; } = [];

    // Conditional logic properties
    public bool HasConditionalLogic { get; set; } = false;
    public string? ConditionalLogicType { get; set; } // e.g., "Show", "Hide", "Enable", "Disable"

    public Guid? TriggerFieldId { get; set; } // The field that triggers the condition
    public FormField? TriggerField { get; set; } // For conditional logic
    public Guid? TriggerValueOptionId { get; set; } // The option that triggers the condition, if applicable
    public FieldOption? TriggerValueOption { get; set; }


    // Validation properties
    public string? ValidationType { get; set; } // e.g., "Range", "Regex", "MinMaxLength"
    public string? ValidationPattern { get; set; } // e.g., regex for phone, email
    public decimal? MinValue { get; set; } // for numbers like BP
    public decimal? MaxValue { get; set; } // for numbers like BP
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public string? CustomErrorMessage { get; set; }

    // Layout properties
    public string? LayoutPosition { get; set; }  // e.g., "Left", "Right", "FullWidth"

}