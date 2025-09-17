// File: Domain/Entities/Clinical/DoctorRecommendation.cs
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;

namespace Salubrity.Domain.Entities.Clinical
{
    public class DoctorRecommendation : BaseAuditableEntity
    {
        public Guid PatientId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid HealthCampId { get; set; }

        // FollowUpRecommendation lookup
        public Guid FollowUpRecommendationId { get; set; }
        public FollowUpRecommendation FollowUpRecommendation { get; set; } = default!;

        // RecommendationType lookup
        public Guid RecommendationTypeId { get; set; }
        public RecommendationType RecommendationType { get; set; } = default!;

        public string? Instructions { get; set; }
    }
}
