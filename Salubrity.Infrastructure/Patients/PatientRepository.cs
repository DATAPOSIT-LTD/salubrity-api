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
    }
}
