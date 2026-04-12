namespace Salubrity.Application.DTOs.HealthCamps;

/// <summary>
/// Camp overview for a specific patient (participant): counts of camps they have attended and upcoming camps they are registered for.
/// </summary>
public class PatientCampOverviewDto
{
    /// <summary>Number of health camps this patient has participated in that have ended (completed).</summary>
    public int CampsAttended { get; set; }

    /// <summary>Number of upcoming health camps this patient is registered for (not yet ended).</summary>
    public int UpcomingCamps { get; set; }
}
