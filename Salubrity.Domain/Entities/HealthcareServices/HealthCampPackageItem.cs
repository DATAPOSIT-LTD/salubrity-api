// File: Salubrity.Domain.Entities.HealthCamps.HealthCampPackageItem.cs

using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthCamps;
using System.ComponentModel.DataAnnotations.Schema;

namespace Salubrity.Domain.Entities.HealthcareServices
{
    public enum PackageItemType
    {
        Service = 0,
        ServiceCategory = 1,
        ServiceSubcategory = 2
    }

    [Table("HealthCampPackageItems")]
    public class HealthCampPackageItem : BaseAuditableEntity
    {
        [ForeignKey("HealthCamp")]
        public Guid HealthCampId { get; set; }
        public virtual HealthCamp HealthCamp { get; set; } = default!;

        public Guid ReferenceId { get; set; } // Could be ServiceId, CategoryId, or SubcategoryId

        public PackageItemType ReferenceType { get; set; }

        // Added to match DB column and enable package linkage
        [ForeignKey("ServicePackage")]
        public Guid? ServicePackageId { get; set; }
        public virtual ServicePackage? ServicePackage { get; set; }
    }
}
