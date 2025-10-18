using System;
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Domain.Entities.HealthCamps
{
    public class HealthCampPackage : BaseAuditableEntity
    {
        public Guid HealthCampId { get; set; }
        public Guid ServicePackageId { get; set; }

        public bool IsActive { get; set; } = true;
        public string? DisplayName { get; set; } // optional label shown at camp
        public decimal? PriceOverride { get; set; }

        // Navigation
        public HealthCamp HealthCamp { get; set; } = null!;
        public ServicePackage ServicePackage { get; set; } = null!;
    }
}
