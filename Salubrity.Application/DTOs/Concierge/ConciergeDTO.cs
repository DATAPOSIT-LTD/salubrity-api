namespace Salubrity.Application.DTOs.Concierge
{
    public class CampServiceStationInfoDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = default!;
        public int QueueLength { get; set; }
        public List<AssignedSubcontractorDto> AssignedSubcontractors { get; set; } = [];
    }

    public class AssignedSubcontractorDto
    {
        public Guid SubcontractorId { get; set; }
        public string SubcontractorName { get; set; } = default!;
    }

    public class CampQueuePriorityDto
    {
        public Guid CheckInId { get; set; }
        public Guid ParticipantId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? CurrentStation { get; set; }
        public int Priority { get; set; }
    }

    public class QueuedParticipantDto
    {
        public string PatientName { get; set; } = default!;
        public string QueueTime { get; set; } = default!;
    }

    public class CampServiceStationWithQueueDto
    {
        public Guid AssignmentId { get; set; }
        public string ServiceStation { get; set; } = default!;
        public int QueueLength { get; set; }
        public string AssignedSubcontractor { get; set; } = default!;
        public List<QueuedParticipantDto> Queue { get; set; } = [];
    }

    public class PatientDetailDto
    {
        public string? ProfilePictureUrl { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Organization { get; set; }
    }

    // ── Live camp progress KPIs ──────────────────────────────────────────────
    public class CampLiveStatsDto
    {
        public int Registered { get; set; }
        public int InStation { get; set; }
        public int Completed { get; set; }
        public int Referred { get; set; }
        public int? Expected { get; set; }
    }

    // ── Participant search ────────────────────────────────────────────────────
    public class ParticipantSearchResultDto
    {
        public Guid ParticipantId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? Organization { get; set; }
        public string? Gender { get; set; }
        public string CurrentStatus { get; set; } = string.Empty;
        public string? CurrentStation { get; set; }
        public List<ParticipantStationJourneyDto> Journey { get; set; } = [];
    }

    public class ParticipantStationJourneyDto
    {
        public string StationName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public int? DurationMinutes { get; set; }
    }

    // ── Station bottleneck ────────────────────────────────────────────────────
    public class StationBottleneckDto
    {
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public int QueueLength { get; set; }
        public double? AvgServiceMinutes { get; set; }
        public double? MaxWaitMinutes { get; set; }
        public List<AssignedSubcontractorDto> AssignedSubcontractors { get; set; } = [];
    }

    // ── Registration timeline ─────────────────────────────────────────────────
    public class RegistrationTimelineDto
    {
        public List<RegistrationSlotDto> Slots { get; set; } = [];
        public int TotalRegistered { get; set; }
        public int PeakCount { get; set; }
        public string? PeakLabel { get; set; }
    }

    public class RegistrationSlotDto
    {
        /// <summary>Hour label in EAT (UTC+3), e.g. "09:00"</summary>
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public int Cumulative { get; set; }
    }
}
