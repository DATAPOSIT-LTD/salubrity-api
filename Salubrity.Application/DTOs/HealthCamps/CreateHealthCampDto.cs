namespace Salubrity.Application.DTOs.HealthCamps;

public class CreateHealthCampDto
{
	public string Name { get; set; } = default!;
	public string? Description { get; set; }
	public string? Location { get; set; }
	public DateTime StartDate { get; set; }
	public DateTime? EndDate { get; set; }
	public TimeSpan? StartTime { get; set; }
	public Guid OrganizationId { get; set; }

	public List<CreateHealthCampPackageItemDto> PackageItems { get; set; } = new();
}
