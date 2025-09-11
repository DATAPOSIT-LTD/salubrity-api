using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Join;

namespace Salubrity.Domain.Entities.Lookup
{
    public class BillingStatus : BaseLookupEntity
    {
        public ICollection<HealthCampParticipant> HealthCampParticipants { get; set; } = [];
    }
}
