namespace Salubrity.Application.DTOs.HealthCamps;

public record SaParticipantDto(Guid UserId, string FullName, string? Email, string? Phone);

public class CampSaStatusDto
{
    public bool RequiresSelfAssessment { get; set; }
    public int TotalParticipants { get; set; }
    public int CompletedCount { get; set; }
    public int NotCompletedCount { get; set; }
    public List<SaParticipantDto> Completed { get; set; } = [];
    public List<SaParticipantDto> NotCompleted { get; set; } = [];
}
