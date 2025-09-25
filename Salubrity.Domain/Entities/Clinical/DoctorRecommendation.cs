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

        // New fields from updated form
        public string? PertinentHistoryFindings { get; set; }
        public string? PertinentClinicalFindings { get; set; }
        public string? DiagnosticImpression { get; set; }
        public string? Conclusion { get; set; }

        // FollowUpRecommendation lookup (Fit to work, Not Fit, etc.)
        public Guid FollowUpRecommendationId { get; set; }
        public FollowUpRecommendation FollowUpRecommendation { get; set; } = default!;

        // RecommendationType lookup (Normal Exam, Advised, Follow up with Specialist)
        public Guid RecommendationTypeId { get; set; }
        public RecommendationType RecommendationType { get; set; } = default!;

        // Specific instructions
        public string? Instructions { get; set; }
    }
}
