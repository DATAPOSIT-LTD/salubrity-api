// File: Salubrity.Domain.Entities.HealthcareServices.ServiceSubcategory.cs

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.IntakeForms;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthcareServices;

[Table("ServiceSubcategories")]
public class ServiceSubcategory : BaseAuditableEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Required]
    public Guid ServiceCategoryId { get; set; }

    [ForeignKey(nameof(ServiceCategoryId))]
    public ServiceCategory ServiceCategory { get; set; } = default!;

    public Guid? IntakeFormId { get; set; }

    [ForeignKey(nameof(IntakeFormId))]
    public IntakeForm? IntakeForm { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    public int? DurationMinutes { get; set; }

    public bool IsActive { get; set; } = true;
}
