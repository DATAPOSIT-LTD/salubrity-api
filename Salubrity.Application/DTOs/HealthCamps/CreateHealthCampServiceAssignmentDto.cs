using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.DTOs.HealthCamps;

public class CreateHealthCampServiceAssignmentDto
{
    public Guid ServiceId { get; set; }
    public Guid SubcontractorId { get; set; }
    public Guid? ProfessionId { get; set; }
}
