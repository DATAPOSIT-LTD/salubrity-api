namespace Salubrity.Application.DTOs.HealthCamps;

public class HealthCampDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public bool IsActive { get; set; }

    public Guid OrganizationId { get; set; }
    public string OrganizationName { get; set; } = default!;
    public int? ExpectedParticipants { get; set; }

    public Guid? ServicePackageId { get; set; }
    public string? ServicePackageName { get; set; }

    public List<HealthCampPackageItemDto> PackageItems { get; set; } = [];
    public List<HealthCampServiceAssignmentDto> ServiceAssignments { get; set; } = [];
    public List<HealthCampParticipantDto> Participants { get; set; } = [];
}
