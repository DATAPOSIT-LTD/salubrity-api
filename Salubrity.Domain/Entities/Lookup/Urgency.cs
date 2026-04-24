// File: Domain/Entities/Lookup/Urgency.cs
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Clinical;

namespace Salubrity.Domain.Entities.Lookup
{
    public class Urgency : BaseLookupEntity
    {
        public ICollection<ServiceReferral> ServiceReferrals { get; set; } = [];
    }
}
