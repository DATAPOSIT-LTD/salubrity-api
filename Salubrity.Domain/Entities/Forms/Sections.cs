using Salubrity.Domain.Entities.FormFields;
using Salubrity.Domain.Entities.Forms;

namespace Salubrity.Domain.Entities.FormSections;
public class FormSection
{
    public Guid Id { get; set; }
    
    public string Name { get; set; }

    public string Description { get; set; }
    public int Order { get; set; }
    
    public int FormId { get; set; }
    public Form Form { get; set; }
    public ICollection<FormField> SectionFields { get; set; } = new List<FormField>();
}
