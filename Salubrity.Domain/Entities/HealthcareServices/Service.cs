using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthcareServices;

[Table("Services")]
public class Service : BaseAuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [MaxLength(255)]
    public string? Description { get; set; }

    public Guid? IndustryId { get; set; }

    [ForeignKey(nameof(IndustryId))]
    public Industry Industry { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
