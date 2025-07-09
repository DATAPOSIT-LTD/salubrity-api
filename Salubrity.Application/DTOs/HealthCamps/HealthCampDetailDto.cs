namespace Salubrity.Application.DTOs.HealthCamps;

public class HealthCampDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string ClientName { get; set; } = default!;
    public string Venue { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime StartDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int ExpectedPatients { get; set; }
    public int SubcontractorCount { get; set; }
    public string PackageName { get; set; } = default!;
    public decimal? PackageCost { get; set; }
    public string InsurerName { get; set; } = default!;
    public List<ServiceStationDto> ServiceStations { get; set; } = new();
}
