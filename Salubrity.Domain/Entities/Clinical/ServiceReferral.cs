// File: Domain/Entities/Clinical/ServiceReferral.cs
using Salubrity.Domain.Common;
using Salubrity.Domain.Entities.Lookup;

namespace Salubrity.Domain.Entities.Clinical
{
    /// <summary>
    /// A referral raised by a service provider at a station for a participant in a camp.
    /// Aggregated into the Individual Final Report.
    /// </summary>
    public class ServiceReferral : BaseAuditableEntity
    {
        public Guid ParticipantId { get; set; }
        public Guid HealthCampId { get; set; }

        // The station the referral was raised at (HealthCampServiceAssignment.Id).
        public Guid ServiceAssignmentId { get; set; }

        // The provider (User.Id) who created the referral.
        public Guid ServiceProviderId { get; set; }

        public string Reason { get; set; } = string.Empty;

        public Guid UrgencyId { get; set; }
        public Urgency Urgency { get; set; } = default!;

        public Guid FollowUpScheduleId { get; set; }
        public FollowUpSchedule FollowUpSchedule { get; set; } = default!;

        // Populated in-memory by the repository after loading. Not mapped to DB.
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? ServiceProviderFullName { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? ResolvedServiceName { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string? Speciality { get; set; }
    }
}
