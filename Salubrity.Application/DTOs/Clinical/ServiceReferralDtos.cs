// File: Application/DTOs/Clinical/ServiceReferralDtos.cs
using System;
using Salubrity.Application.DTOs.Lookups;

namespace Salubrity.Application.DTOs.Clinical
{
    public class CreateServiceReferralDto
    {
        public Guid ParticipantId { get; set; }
        public Guid ServiceAssignmentId { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid UrgencyId { get; set; }
        public Guid FollowUpScheduleId { get; set; }
    }

    public class UpdateServiceReferralDto
    {
        public string Reason { get; set; } = string.Empty;
        public Guid UrgencyId { get; set; }
        public Guid FollowUpScheduleId { get; set; }
    }

    public class ServiceReferralResponseDto
    {
        public Guid Id { get; set; }
        public Guid ParticipantId { get; set; }
        public Guid HealthCampId { get; set; }
        public Guid ServiceAssignmentId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public Guid ServiceProviderId { get; set; }
        public string ServiceProviderName { get; set; } = string.Empty;
        public string Speciality { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public BaseLookupResponse Urgency { get; set; } = default!;
        public BaseLookupResponse FollowUpSchedule { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
    }
}
