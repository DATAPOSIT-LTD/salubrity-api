using Salubrity.Domain.Entities.HealthcareServices;

namespace Salubrity.Application.DTOs.HealthCamps;

// public class HealthCampServiceAssignmentDto
// {
//     public Guid Id { get; set; }
//     public Guid HealthCampId { get; set; }

//     public Guid ServiceId { get; set; }
//     public string ServiceName { get; set; } = default!;

//     public Guid SubcontractorId { get; set; }
//     public string SubcontractorName { get; set; } = default!;
// }



public class HealthCampServiceAssignmentDto
{
    public Guid AssignmentId { get; set; }
    public PackageItemType AssignmentType { get; set; }

    public Guid SubcontractorId { get; set; }
    public Guid? ProfessionId { get; set; }

    // Optional hydrated fields
    public string? AssignmentName { get; set; }
    public string? SubcontractorName { get; set; }
}

