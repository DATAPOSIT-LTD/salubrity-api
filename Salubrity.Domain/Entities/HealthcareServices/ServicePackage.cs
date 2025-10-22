// File: Salubrity.Domain.Entities.HealthcareServices.ServicePackage.cs

using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthcareServices;

[Table("ServicePackages")]
public class ServicePackage : BaseAuditableEntity
{
    [Required, MaxLength(100)]
    public string Name { get; set; } = default!;

    [MaxLength(255)]
    public string? Description { get; set; }

    [Column(TypeName = "numeric(10,2)")]
    public decimal? Price { get; set; }

    [MaxLength(100)]
    public string? RangeOfPeople { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<HealthCampPackageItem> Items { get; set; } = [];

}
