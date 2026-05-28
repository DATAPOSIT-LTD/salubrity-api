namespace Salubrity.Application.DTOs.HealthCamps;

public class CampBillingItemDto
{
    public Guid ParticipantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PackageName { get; set; }
    public Guid? BillingStatusId { get; set; }
    public string BillingStatusName { get; set; } = "Not Billed";
    public bool IsBilled { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class BulkAssignPackageDto
{
    public Guid HealthCampPackageId { get; set; }
    /// <summary>If true, also re-assign participants who already have a different package.</summary>
    public bool OverwriteExisting { get; set; }
}

public class BulkAssignPackageResultDto
{
    public int AssignedCount { get; set; }
    public int SkippedCount { get; set; }
}
