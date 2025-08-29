namespace Salubrity.Application.DTOs.HealthCamps
{
    public class OrganizationCampListDto
    {
        public Guid Id { get; set; }
        public string CampName { get; set; } = default!;
        public DateTime CampDate { get; set; }
        public string CampVenue { get; set; } = default!;
        public string CampStatus { get; set; } = default!;
        public int NumberOfPatients { get; set; }
    }
}