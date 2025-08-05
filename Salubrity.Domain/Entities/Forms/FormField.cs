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

}