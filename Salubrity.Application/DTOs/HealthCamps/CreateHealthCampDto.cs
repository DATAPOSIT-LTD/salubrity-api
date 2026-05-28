namespace Salubrity.Application.DTOs.HealthCamps
{
	public class CreateHealthCampDto
	{
		public string Name { get; set; } = default!;
		public string? Description { get; set; }
		public string? Location { get; set; }
		public Guid OrganizationId { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public TimeSpan? StartTime { get; set; }
		public int? ExpectedParticipants { get; set; }

		public bool RequiresSelfAssessment { get; set; } = false;

		// Multiple package blocks — each with its own items & services
		public List<CreateCampPackageDto> Packages { get; set; } = new();
	}
}

