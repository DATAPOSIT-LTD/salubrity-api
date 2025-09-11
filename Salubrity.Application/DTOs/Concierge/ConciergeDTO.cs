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
}
