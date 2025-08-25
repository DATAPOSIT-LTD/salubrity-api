namespace Salubrity.Application.DTOs.HealthCamps;

public class MyCampListItemDto
{
    public Guid CampId { get; set; }
    public string? CampName { get; set; }

    public string? Organization { get; set; }       // e.g., "Tetra Pack"
    public string? PackageServices { get; set; }    // e.g., "Gold"
    public int NumberOfServices { get; set; }       // e.g., 9

    public string? Venue { get; set; }              // or Location.Name
    public DateTime? StartDate { get; set; }        // UTC
    public DateTime? EndDate { get; set; }          // UTC
    public string Status { get; set; } = "Ongoing"; // Planned|Ongoing|Completed|Suspended
}

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
}
