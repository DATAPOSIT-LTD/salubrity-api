using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.DTOs.HealthCamps;

public class HealthCampPackageItemDto
{
    public Guid Id { get; set; }

    public Guid HealthCampId { get; set; }

    public Guid ReferenceId { get; set; }

    public PackageItemType ReferenceType { get; set; }

    public string ReferenceName { get; set; } = default!; // Resolved name from service/category/subcategory
}
