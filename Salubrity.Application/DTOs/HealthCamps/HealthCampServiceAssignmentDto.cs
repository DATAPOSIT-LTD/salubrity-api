namespace Salubrity.Application.DTOs.HealthCamps;

public class HealthCampServiceAssignmentDto
{
    public Guid Id { get; set; }
    public Guid HealthCampId { get; set; }

    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = default!;

    public Guid SubcontractorId { get; set; }
    public string SubcontractorName { get; set; } = default!;
}
