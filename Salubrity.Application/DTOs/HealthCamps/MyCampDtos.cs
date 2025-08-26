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
public class MyCampServiceDto
{
    public Guid CampAssignmentId { get; set; }

    public Guid SubcontractorId { get; set; }
    public Guid? ProfessionId { get; set; }

    public string ServedBy { get; set; } = default!; // e.g., Dr. John Doe
    public string? Profession { get; set; }          // e.g., Cardiologist

    public Guid ServiceId { get; set; }
    public string ServiceName { get; set; } = default!;
    public string? StationName { get; set; }          // alias on assignment, defaults to service name
    public string? Description { get; set; }
    public bool IsRequired { get; set; }
    public bool IsCompleted { get; set; }             // for the current participant
    public int? Order { get; set; }
    public string? IconUrl { get; set; }              // if you store one on assignment/service
}
public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
}
