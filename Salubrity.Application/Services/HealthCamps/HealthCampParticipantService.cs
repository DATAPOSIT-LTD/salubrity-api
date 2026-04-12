using Salubrity.Application.Common.Interfaces.Repositories;
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
        var participant = await _repo.GetParticipantAsync(
            dto.CampId,
            dto.ParticipantId,
            ct);

        if (participant == null)
            throw new NotFoundException(
                "HealthCampParticipant",
                $"Camp={dto.CampId}, Participant={dto.ParticipantId}");

        // ------------------------------------------------
        // Resolve PatientId via existing repository method
        // ------------------------------------------------
        var patientId = await _repo.GetPatientIdByParticipantIdAsync(
            participant.Id,
            ct);

        if (!patientId.HasValue)
            throw new InvalidOperationException(
                $"HealthCampParticipant {participant.Id} is not linked to a Patient");

        // ------------------------------------------------
        // Resolve camp services
        // ------------------------------------------------
        var serviceIds = await _repo.GetServiceIdsForCampAsync(
            dto.CampId,
            ct);

        // ------------------------------------------------
        // Invalidate form submissions
        // ------------------------------------------------
        var responses = await _repo.GetFormResponsesForPatientAndServicesAsync(
            patientId.Value,
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

        // ------------------------------------------------
        // Remove participant
        // ------------------------------------------------
        participant.IsDeleted = true;
        participant.DeletedAt = DateTime.UtcNow;
        participant.DeletedBy = actingUserId;
        participant.Notes = dto.Notes;

        await _repo.SaveChangesAsync(ct);
    }


}
