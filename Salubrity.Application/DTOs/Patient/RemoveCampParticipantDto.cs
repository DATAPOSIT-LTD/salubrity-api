namespace Salubrity.Application.DTOs.HealthCamps.Participants;

public class RemoveCampParticipantDto
{
    public Guid CampId { get; set; }
    public Guid PatientId { get; set; }

    // Optional but recommended
    public Guid? RemovalReasonId { get; set; }
    public string? Notes { get; set; }
}
