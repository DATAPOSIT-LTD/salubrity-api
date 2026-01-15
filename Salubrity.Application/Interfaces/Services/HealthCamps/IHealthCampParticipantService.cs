using Salubrity.Application.DTOs.HealthCamps.Participants;

public interface IHealthCampParticipantService
{
    Task RemovePatientFromCampAsync(
        RemoveCampParticipantDto dto,
        Guid actingUserId,
        CancellationToken ct);
}
