namespace Salubrity.Application.DTOs.HealthCamps.Participants;

public class RemoveCampParticipantDto
{
    public Guid CampId { get; set; }
    public Guid ParticipantId { get; set; }

    // Optional
    public Guid? RemovalReasonId { get; set; }
    public string? Notes { get; set; }
}
