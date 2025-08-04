using System.ComponentModel.DataAnnotations.Schema;
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.IntakeForms;

namespace Salubrity.Domain.Entities.Lookup;

[Table("FieldTypes")]
public class FieldType : BaseLookupEntity
{
    // Reserved for future use (e.g., front-end rendering hints)
    public string? ComponentHint { get; set; }

    public ICollection<IntakeFormField> Fields { get; set; } = new List<IntakeFormField>();
}
