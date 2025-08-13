using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.IntakeForms;

[Table("IntakeForms")]
public class IntakeForm : BaseAuditableEntity
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = default!;

    [MaxLength(500)]
    public string? Description { get; set; }


    public bool IsActive { get; set; } = true;


    public ICollection<IntakeFormSection> Sections { get; set; } = [];
    public ICollection<IntakeFormField> Fields { get; set; } = [];

    public ICollection<IntakeFormVersion> Versions { get; set; } = [];
}
