namespace Salubrity.Application.DTOs.HealthCamps;

public class CampParticipantContactDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
}
