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
        public Guid ParticipantId { get; set; }
        public string PatientName { get; set; } = default!;
        //public Guid PatientId { get; set; }
        public string? CurrentStation { get; set; }
        public int Priority { get; set; }
    }
}
