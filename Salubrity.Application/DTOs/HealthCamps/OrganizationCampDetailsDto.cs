namespace Salubrity.Application.DTOs.HealthCamps;

public class OrganizationCampDetailsDto
{
    public string CampName { get; set; } = default!;
    public DateTime StartDateTime { get; set; }

    // Camp Details
    public string ClientName { get; set; } = default!;
    public string Venue { get; set; } = default!;
    public int ExpectedPatients { get; set; }
    public int SubcontractorCount { get; set; }
    public bool SubcontractorsReady { get; set; }

    // Finance Details
    public string Insurer { get; set; } = default!;
    public string PackageName { get; set; } = default!;
    public string PackageCost { get; set; } = default!;
}
