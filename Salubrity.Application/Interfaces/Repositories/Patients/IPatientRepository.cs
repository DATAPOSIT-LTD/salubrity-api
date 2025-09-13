// File: Salubrity.Application/Interfaces/Repositories/Patients/IPatientRepository.cs
using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Application.Interfaces.Repositories.Patients
{
    public interface IPatientRepository
    {
        Task AddAsync(Patient patient, CancellationToken ct = default);
        Task<Patient?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
        /// <summary>
        /// Get the PatientId by a patient number (external identifier).
        /// Returns null if not found.
        /// </summary>
        Task<Guid?> GetPatientIdByPatientNumberAsync(string patientNumber, CancellationToken ct = default);
        Task<List<Patient>> GetAllPatientsAsync(CancellationToken ct);
        Task<List<Patient>> GetPatientsByCampAsync(Guid campId, CancellationToken ct = default);

    }
}
