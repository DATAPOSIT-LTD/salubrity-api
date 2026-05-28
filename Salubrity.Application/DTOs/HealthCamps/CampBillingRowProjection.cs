namespace Salubrity.Application.DTOs.HealthCamps;

public class CampBillingRowProjection
{
    public Guid ParticipantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? PackageName { get; set; }
    public Guid? BillingStatusId { get; set; }
    public string? BillingStatusName { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
