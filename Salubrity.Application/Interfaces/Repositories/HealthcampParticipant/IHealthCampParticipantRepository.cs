using Salubrity.Domain.Entities.IntakeForms;
using Salubrity.Domain.Entities.Join;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Salubrity.Application.Common.Interfaces.Repositories
{
    public interface IHealthCampParticipantRepository
    {
        Task<Guid?> GetPatientIdByParticipantIdAsync(Guid participantId, CancellationToken ct = default);
        Task<bool> IsParticipantLinkedToCampAsync(Guid campId, Guid userId, CancellationToken ct = default);
        Task AddParticipantAsync(Domain.Entities.Join.HealthCampParticipant participant, CancellationToken ct = default);
        Task<HealthCampParticipant?> GetParticipantAsync(Guid campId, Guid participantId, CancellationToken ct = default);
        Task UpdateParticipantAsync(HealthCampParticipant participant, CancellationToken ct = default);
        Task<Guid?> GetParticipantIdByPatientIdAsync(Guid patientId, CancellationToken ct = default);
        Task<HealthCampParticipant?> GetParticipantWithBillingStatusAsync(Guid campId, Guid participantId, CancellationToken ct = default);
        Task<HealthCampParticipant?> GetParticipantWithBillingStatusByIdAsync(Guid participantId, CancellationToken ct = default);

        Task<List<Guid>> GetServiceIdsForCampAsync(
            Guid campId,
            CancellationToken ct);

        Task<List<IntakeFormResponse>> GetFormResponsesForPatientAndServicesAsync(
            Guid patientId,
            List<Guid> serviceIds,
            CancellationToken ct);

        Task SaveChangesAsync(CancellationToken ct);
        /// <summary>
        /// Loads a participant with all navigation properties needed for the report demographics
        /// (User + Gender, Patient + PrimaryOrganization, HealthCamp).
        /// </summary>
        Task<HealthCampParticipant?> GetParticipantWithDemographicsAsync(Guid participantId, CancellationToken ct = default);

        /// <summary>
        /// Returns the participantId for the given user in the given camp, or null if not enrolled.
        /// </summary>
        Task<Guid?> GetParticipantIdByUserAndCampAsync(Guid userId, Guid campId, CancellationToken ct = default);

        Task<List<Salubrity.Application.DTOs.HealthCamps.CampBillingRowProjection>> GetBillingRowsAsync(Guid campId, CancellationToken ct = default);
        Task<List<Guid>> GetParticipantIdsForBulkAssignAsync(Guid campId, bool overwriteExisting, CancellationToken ct = default);
    }



}
