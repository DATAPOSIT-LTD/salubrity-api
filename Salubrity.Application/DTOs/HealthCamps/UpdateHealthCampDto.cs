namespace Salubrity.Application.DTOs.HealthCamps;

public class UpdateHealthCampDto
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan? StartTime { get; set; }
    public bool IsActive { get; set; }
    public Guid OrganizationId { get; set; }

    public List<UpdateHealthCampPackageItemDto> PackageItems { get; set; } = new();
}
