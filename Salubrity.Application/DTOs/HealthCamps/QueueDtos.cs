// File: Salubrity.Application/DTOs/Camps/QueueDtos.cs
namespace Salubrity.Application.DTOs.HealthCamps;

public class CheckInRequestDto
{
    public Guid CampId { get; set; }
    public Guid AssignmentId { get; set; } // HealthCampServiceAssignmentId (station)
    public int? Priority { get; set; }     // optional, default 0
}

public class QueuePositionDto
{
    public Guid AssignmentId { get; set; }
    public string StationName { get; set; } = default!;
    public int QueueLength { get; set; }     // total in 'Queued'
    public int YourPosition { get; set; }    // 1-based, considering priority
    public string Status { get; set; } = "Queued"; // Queued/InService/Completed/Canceled
    public List<CoveredServiceDto> CoveredServices { get; set; } = new();
}

public class CoveredServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Type { get; set; } = default!; // Service / Category / Subcategory
    public string Status { get; set; } = "Queued"; // Queued / InService / Completed
}



public class CheckInStateDto
{
    public Guid CampId { get; set; }
    public Guid? ActiveAssignmentId { get; set; }
    public string? ActiveStationName { get; set; }
    public string Status { get; set; } = "None"; // Derived aggregate
    public List<CoveredServiceDto> CoveredServices { get; set; } = new();
}

public static class CampQueueStatus
{
    public const string Queued = "Queued";
    public const string InService = "InService";
    public const string Completed = "Completed";
    public const string Canceled = "Canceled";
}
