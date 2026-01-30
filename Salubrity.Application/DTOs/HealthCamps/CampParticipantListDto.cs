// Salubrity.Application/DTOs/HealthCamps/CampParticipantListDto.cs
namespace Salubrity.Application.DTOs.HealthCamps;

public class CampParticipantListDto
{
    // --------------------------------------------------
    // Identity
    // --------------------------------------------------
    public Guid Id { get; set; }               // HealthCampParticipant Id
    public Guid UserId { get; set; }
    public Guid? PatientId { get; set; }

    // --------------------------------------------------
    // Participant Info
    // --------------------------------------------------
    public string FullName { get; set; } = default!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    public string CompanyName { get; set; } = default!;
    public DateTime? ParticipatedAt { get; set; }

    // --------------------------------------------------
    // Package (if applicable)
    // --------------------------------------------------
    public Guid? PackageId { get; set; }
    public string? PackageName { get; set; }

    // --------------------------------------------------
    // Service status
    // --------------------------------------------------
    // When querying by a specific service → populated
    // When querying camp-wide → null
    public bool? Served { get; set; }

    // --------------------------------------------------
    // Camp-wide service completion
    // --------------------------------------------------
    public List<ServiceCompletionDto?> CompletedServices { get; set; } = new();
}

public class ServiceCompletionDto
{
    // HealthCampServiceAssignment reference
    public Guid ServiceAssignmentId { get; set; }

    // Canonical resolved service
    public Guid ResolvedServiceId { get; set; }
    public string ServiceName { get; set; } = default!;

    // When the service was completed (if available)
    public DateTime? ServedAt { get; set; }
}
