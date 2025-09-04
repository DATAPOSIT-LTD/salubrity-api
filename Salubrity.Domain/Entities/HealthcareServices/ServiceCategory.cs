// File: Salubrity.Domain.Entities.HealthcareServices.ServiceCategory.cs

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.IntakeForms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthcareServices;

[Table("ServiceCategories")]
public class ServiceCategory : BaseAuditableEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    public Guid ServiceId { get; set; }

    [ForeignKey(nameof(ServiceId))]
    public Service Service { get; set; } = default!;

    public Guid? IntakeFormId { get; set; }

    [ForeignKey(nameof(IntakeFormId))]
    public IntakeForm? IntakeForm { get; set; }

    public decimal? Price { get; set; }
    public int? DurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<ServiceSubcategory> Subcategories { get; set; } = [];
}
