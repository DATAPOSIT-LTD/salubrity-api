using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.HealthCamps;

namespace Salubrity.Domain.Entities.Lookup
{
    public class HealthCampStatus : BaseLookupEntity
    {
        // Additional properties or methods specific to CampStatus can be added here
        public ICollection<HealthCamp> HealthCamps { get; set; } = [];
    }
}