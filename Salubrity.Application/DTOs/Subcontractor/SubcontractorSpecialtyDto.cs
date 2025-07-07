namespace Salubrity.Application.DTOs.Subcontractor
{
    public class SubcontractorSpecialtyDto
    {
        public Guid Id { get; set; }

        public Guid SubcontractorId { get; set; }

        public Guid ServiceId { get; set; }

        public string ServiceName { get; set; } = default!;
    }
}
