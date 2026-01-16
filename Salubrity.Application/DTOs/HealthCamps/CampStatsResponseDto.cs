namespace Salubrity.Application.DTOs.HealthCamps;

public class CampStatsResponseDto
{
    public Guid CampId { get; set; }
    public string CampName { get; set; } = default!;
    public bool IsActive { get; set; }

    public CampStatsCountsDto Counts { get; set; } = new();
    public CampStatsPackageDto Packages { get; set; } = new();
    public CampStatsServiceDto Services { get; set; } = new();
}

public class CampStatsCountsDto
{
    public int Participants { get; set; }
    public int Staff { get; set; }
    // public int Vendors { get; set; }
    // public int Visitors { get; set; }
}

public class CampStatsPackageDto
{
    public int TotalPackages { get; set; }
    public int AssignedPackages { get; set; }
    public int UnassignedParticipants { get; set; }
}
public class CampStatsServiceDto
{
    public int TotalServices { get; set; }
    public int ServedServices { get; set; }
    public int PendingServices { get; set; }
}
