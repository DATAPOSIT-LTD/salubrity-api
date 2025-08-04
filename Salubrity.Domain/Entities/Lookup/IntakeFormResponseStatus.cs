using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;
using Salubrity.Domain.Entities.IntakeForms;


namespace Salubrity.Domain.Entities.Lookup;

[Table("IntakeFormResponseStatuses")]
public class IntakeFormResponseStatus : BaseLookupEntity
{
    public ICollection<IntakeFormResponse> Responses { get; set; } = new List<IntakeFormResponse>();
}
