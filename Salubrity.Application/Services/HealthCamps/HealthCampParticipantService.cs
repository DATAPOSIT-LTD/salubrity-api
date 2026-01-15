using Salubrity.Application.DTOs.HealthCamps.Participants;
using Salubrity.Shared.Exceptions;

public class HealthCampParticipantService : IHealthCampParticipantService
{
    private readonly IHealthCampParticipantRepository _repo;

    public HealthCampParticipantService(
        IHealthCampParticipantRepository repo)
    {
        _repo = repo;
    }

    public async Task RemovePatientFromCampAsync(
        RemoveCampParticipantDto dto,
        Guid actingUserId,
        CancellationToken ct)
    {
        var participant = await _repo.GetPatientParticipantAsync(
            dto.CampId,
            dto.PatientId,
            ct);

        if (participant == null)
            throw new NotFoundException(
                "HealthCampParticipant",
                $"Camp={dto.CampId}, Patient={dto.PatientId}");

        // --------------------------------------
        // 1. Resolve camp services
        // --------------------------------------
        var serviceIds = await _repo
            .GetServiceIdsForCampAsync(dto.CampId, ct);

        // --------------------------------------
        // 2. Invalidate related form responses
        // --------------------------------------
        var responses = await _repo
            .GetFormResponsesForPatientAndServicesAsync(
                dto.PatientId,
                serviceIds,
                ct);

        foreach (var response in responses)
        {
            response.IsDeleted = true;
            response.DeletedAt = DateTime.UtcNow;
            response.DeletedBy = actingUserId;

            foreach (var field in response.FieldResponses)
            {
                field.IsDeleted = true;
                field.DeletedAt = DateTime.UtcNow;
                field.DeletedBy = actingUserId;
            }
        }

        // --------------------------------------
        // 3. Remove participant
        // --------------------------------------
        participant.IsDeleted = true;
        participant.DeletedAt = DateTime.UtcNow;
        participant.DeletedBy = actingUserId;
        // participant.RemovalReasonId = dto.RemovalReasonId;
        // participant.RemovalNotes = dto.Notes;

        await _repo.SaveChangesAsync(ct);
    }
}
