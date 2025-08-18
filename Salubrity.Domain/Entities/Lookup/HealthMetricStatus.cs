using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Join;

namespace Salubrity.Domain.Entities.Lookup
{
    public class HealthMetricStatus : BaseLookupEntity
    {
        // Additional properties or methods specific to CampStatus can be added here
        public ICollection<HealthCampParticipant> HealthCampParticipants { get; set; } = [];

    }
}