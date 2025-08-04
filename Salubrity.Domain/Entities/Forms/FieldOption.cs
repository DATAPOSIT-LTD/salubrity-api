using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.FormFields;

namespace Salubrity.Domain.Entities.FormsOptions;
public class FieldOption : BaseAuditableEntity
{


    public Guid FormFieldId { get; set; }
    public FormField? FormField { get; set; }
    public string? Value { get; set; }
    public string? DisplayText { get; set; }
    
}