using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthcareServices;

[Table("ServiceSubcategories")]
public class ServiceSubcategory : BaseAuditableEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public Guid ServiceCategoryId { get; set; }
    public decimal Price { get; set; }
    public int? DurationMinutes { get; set; }

    public ServiceCategory ServiceCategory { get; set; } = default!;
}
