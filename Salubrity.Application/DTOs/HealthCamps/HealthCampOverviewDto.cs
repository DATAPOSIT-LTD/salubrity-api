namespace Salubrity.Application.DTOs.HealthCamps
{
    public class HealthCampOverviewDto
    {
        public int OnboardedOrganizations { get; set; }
        public int CompletedCamps { get; set; }
        public int UpcomingCamps { get; set; }
        public int TotalPatients { get; set; }
    }
}