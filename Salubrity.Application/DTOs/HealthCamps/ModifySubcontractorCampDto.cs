namespace Salubrity.Application.DTOs.HealthCamps
{
    public class ModifySubcontractorCampDto
    {
        public Guid SubcontractorId { get; set; }
        public List<Guid> ServiceIds { get; set; } = new();
    }
}
