// File: Salubrity.Infrastructure/Repositories/Patients/PatientRepository.cs
using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Patients;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Patients
{
    public class PatientRepository : IPatientRepository
    {
        private readonly AppDbContext _db;

        public PatientRepository(AppDbContext db) => _db = db;

        public async Task AddAsync(Patient patient, CancellationToken ct = default)
        {
            await _db.Patients.AddAsync(patient, ct);
            await _db.SaveChangesAsync(ct);
        }

        public Task<Patient?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
            _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.UserId == userId, ct);

        public Task<Patient?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            _db.Patients.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);

        /// <summary>
        /// Get PatientId by PatientNumber (external identifier)
        /// </summary>
        public async Task<Guid?> GetPatientIdByPatientNumberAsync(string patientNumber, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(patientNumber))
                return null;

            var patient = await _db.Patients
                .Where(p => p.PatientNumber == patientNumber.Trim())
                .Select(p => new { p.Id })
                .FirstOrDefaultAsync(ct);

            return patient?.Id;
        }
        /// <summary>
        /// Fetch all patients with user details (names) for Excel lab templates.
        /// </summary>
        public async Task<List<Patient>> GetAllPatientsAsync(CancellationToken ct = default)
        {
            return await _db.Patients
                .Include(p => p.User)   // Required to get FirstName, MiddleName, LastName
                .OrderBy(p => p.PatientNumber)
                .ToListAsync(ct);
        }
        public async Task<List<Patient>> GetPatientsByCampAsync(Guid campId, CancellationToken ct = default)
        {
            return await _db.HealthCampParticipants
                .Where(p => p.HealthCampId == campId && !p.IsDeleted)
                .Include(p => p.Patient)
                    .ThenInclude(p => p.User)
                .Select(p => p.Patient)
                .Where(p => p != null && !p.IsDeleted)
                .ToListAsync(ct)
                .ContinueWith(t => t.Result!.Where(p => p != null).Cast<Patient>().ToList(), ct);
        }

    }

}
