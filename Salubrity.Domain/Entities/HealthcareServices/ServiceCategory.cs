using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Salubrity.Domain.Common;

namespace Salubrity.Domain.Entities.HealthcareServices;

[Table("ServiceCategories")]
public class ServiceCategory : BaseAuditableEntity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    [ForeignKey("Service")]
    public Guid ServiceId { get; set; }
    public virtual Service Service { get; set; } = default!;

    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;
}
