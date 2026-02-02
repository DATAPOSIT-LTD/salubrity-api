namespace Salubrity.Application.DTOs.HealthCamps;

/// <summary>
/// Camp overview for a specific patient (camps attended and upcoming).
/// </summary>
public class PatientCampOverviewDto
{
    /// <summary>Number of camps this patient has attended (completed camps).</summary>
    public int CampsAttended { get; set; }

    /// <summary>Number of upcoming camps this patient is registered for.</summary>
    public int UpcomingCamps { get; set; }
}
