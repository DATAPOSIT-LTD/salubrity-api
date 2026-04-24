// File: Application/DTOs/HealthCamps/MyStationAssignmentDto.cs
namespace Salubrity.Application.DTOs.HealthCamps
{
    /// <summary>
    /// One station assignment for the current subcontractor in a given camp.
    /// Used by the provider-side referral form so they can submit a referral
    /// scoped to the station they are working.
    /// </summary>
    public class MyStationAssignmentDto
    {
        public Guid CampAssignmentId { get; set; }
        public Guid ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string? StationName { get; set; }
        public Guid? ProfessionId { get; set; }
        public string? Profession { get; set; }
    }
}
