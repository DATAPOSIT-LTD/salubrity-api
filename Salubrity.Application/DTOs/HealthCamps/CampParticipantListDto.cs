// Salubrity.Application/DTOs/HealthCamps/CampParticipantListDto.cs
namespace Salubrity.Application.DTOs.HealthCamps;

public class CampParticipantListDto
{
    public Guid Id { get; set; }               // HealthCampParticipant Id
    public Guid UserId { get; set; }
    public Guid? PatientId { get; set; }
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string CompanyName { get; set; } = default!; // camp's Organization
    public bool Served { get; set; }
    public DateTime? ParticipatedAt { get; set; }

    public Guid? PackageId { get; set; }                // Participant Package
    public string? PackageName { get; set; }

    // NEW: list of service names or IDs participant completed
    public List<ServiceCompletionDto> CompletedServices { get; set; } = new();
}

public class ServiceCompletionDto
{
    public Guid ServiceAssignmentId { get; set; }
    public string? ServiceName { get; set; }
    public DateTime? ServedAt { get; set; }
}
