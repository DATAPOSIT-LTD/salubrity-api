namespace Salubrity.Application.DTOs.Organizations
{
    public class OrganizationResponseDto
    {
        public Guid Id { get; set; }
        public string BusinessName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string? ClientLogoPath { get; set; }
        public Guid? InsuranceProvidingId { get; set; }
        public Guid? ContactPersonId { get; set; }
        public Guid? StatusId { get; set; }
    }
}
