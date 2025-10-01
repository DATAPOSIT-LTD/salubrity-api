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
        // Part 1: Contact details
        public string? ProfilePictureUrl { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        //public string? Residence { get; set; }
        //public string? NextOfKin { get; set; }
        //public string? Relationship { get; set; }

        // Part 2: General Overview
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Organization { get; set; }
        //public string? Allergies { get; set; }
    }
}
