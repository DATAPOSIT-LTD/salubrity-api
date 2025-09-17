// File: Domain/Entities/Lookup/FollowUpRecommendation.cs
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Clinical;

namespace Salubrity.Domain.Entities.Lookup
{
    public class FollowUpRecommendation : BaseLookupEntity
    {
        public ICollection<DoctorRecommendation> DoctorRecommendations { get; set; } = [];
    }
}
