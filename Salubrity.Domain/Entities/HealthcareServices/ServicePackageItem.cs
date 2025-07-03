// File: Salubrity.Domain.Entities.HealthcareServices.ServicePackageItem.cs

using Salubrity.Domain.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthcareServices;

public enum PackageItemType
{
    Service = 0,
    ServiceCategory = 1,
    ServiceSubcategory = 2
}

[Table("ServicePackageItems")]
public class ServicePackageItem : BaseAuditableEntity
{
    public Guid ServicePackageId { get; set; }

    [ForeignKey(nameof(ServicePackageId))]
    public ServicePackage ServicePackage { get; set; } = default!;

    public Guid ReferenceId { get; set; }  // Refers to Service, Category, or Subcategory

    public PackageItemType ReferenceType { get; set; }
}
