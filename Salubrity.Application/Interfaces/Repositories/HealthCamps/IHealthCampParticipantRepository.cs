using Salubrity.Domain.Entities.IntakeForms;

public interface IHealthCampParticipantRepository
{
    Task<Salubrity.Domain.Entities.Join.HealthCampParticipant?> GetPatientParticipantAsync(
        Guid campId,
        Guid patientId,
        CancellationToken ct);

    Task<List<Guid>> GetServiceIdsForCampAsync(
        Guid campId,
        CancellationToken ct);

    Task<List<IntakeFormResponse>> GetFormResponsesForPatientAndServicesAsync(
        Guid patientId,
        List<Guid> serviceIds,
        CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
