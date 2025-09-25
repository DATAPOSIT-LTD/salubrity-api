// File: Domain/Entities/Lookup/RecommendationType.cs
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Clinical;

namespace Salubrity.Domain.Entities.Lookup
{
    public class RecommendationType : BaseLookupEntity
    {
        public ICollection<DoctorRecommendation> DoctorRecommendations { get; set; } = [];
    }
}
