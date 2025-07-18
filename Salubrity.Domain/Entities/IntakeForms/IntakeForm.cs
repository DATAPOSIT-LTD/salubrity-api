using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeForms")]
public class IntakeForm : BaseAuditableEntity
{
    [Required, MaxLength(150)]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    public ICollection<IntakeFormVersion> Versions { get; set; } = new List<IntakeFormVersion>();
}
