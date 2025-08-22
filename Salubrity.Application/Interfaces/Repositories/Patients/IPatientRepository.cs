// File: Salubrity.Application/Interfaces/Repositories/Patients/IPatientRepository.cs
using Salubrity.Domain.Entities.Identity;

namespace Salubrity.Application.Interfaces.Repositories.Patients
{
    public interface IPatientRepository
    {
        Task AddAsync(Patient patient, CancellationToken ct = default);
        Task<Patient?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
        Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default);
    }
}
