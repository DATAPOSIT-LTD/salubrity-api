using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.FormFields;
using Salubrity.Domain.Entities.FormSections;

namespace Salubrity.Domain.Entities.Forms;

public class Form : BaseAuditableEntity
{
    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public bool IsActive { get; set; } = true;
    

    public ICollection<FormSection> Sections { get; set; } = new List<FormSection>();
}