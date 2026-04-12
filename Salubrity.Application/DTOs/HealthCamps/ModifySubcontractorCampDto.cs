namespace Salubrity.Application.DTOs.HealthCamps
{
    public class ModifySubcontractorCampDto
    {
        public Guid SubcontractorId { get; set; }

        // Each entry represents:
        // one service + one profession (station role)
        public List<SubcontractorServiceAssignmentDto> Assignments { get; set; } = new();
    }

    public class SubcontractorServiceAssignmentDto
    {
        public Guid ServiceId { get; set; }
        public Guid ProfessionId { get; set; }
    }
}
