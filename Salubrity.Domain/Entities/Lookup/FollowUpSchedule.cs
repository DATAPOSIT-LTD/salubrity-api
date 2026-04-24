// File: Domain/Entities/Lookup/FollowUpSchedule.cs
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Clinical;

namespace Salubrity.Domain.Entities.Lookup
{
    public class FollowUpSchedule : BaseLookupEntity
    {
        public ICollection<ServiceReferral> ServiceReferrals { get; set; } = [];
    }
}
