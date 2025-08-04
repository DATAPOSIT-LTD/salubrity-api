using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeFormSections")]
public class IntakeFormSection : BaseAuditableEntity
{
    [Required]
    public Guid IntakeFormVersionId { get; set; }

    [ForeignKey(nameof(IntakeFormVersionId))]
    public IntakeFormVersion Version { get; set; } = default!;

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }

    public int Order { get; set; }

    public ICollection<IntakeFormField> Fields { get; set; } = new List<IntakeFormField>();
}
