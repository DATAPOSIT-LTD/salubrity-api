namespace Salubrity.Application.DTOs.Organizations
{
    public class OrganizationOverviewDto
    {
        public int OnboardedOrganizations { get; set; }
        public int TotalPatientsOnboarded { get; set; }
        public int TotalPatientsChecked { get; set; }
        public decimal TotalPaidClaims { get; set; }
    }
}
