namespace Salubrity.Application.DTOs.HealthCamps;

public class ParticipantStationStatusDto
{
    public string ServiceName { get; set; } = default!;
    public DateTimeOffset? CheckInTime { get; set; }
    public DateTimeOffset? CheckOutTime { get; set; }
    public string Status { get; set; } = default!;
}
