using Microsoft.EntityFrameworkCore;
using Salubrity.Application.Interfaces.Repositories.Patients;
using Salubrity.Domain.Entities.Identity;
using Salubrity.Domain.Entities.Patients;
using Salubrity.Infrastructure.Persistence;

namespace Salubrity.Infrastructure.Repositories.Patients
{
    public class PatientNumberRepository : IPatientNumberRepository
    {
        private readonly AppDbContext _db;

        public PatientNumberRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task<string?> GetLastPatientNumberForYearAsync(int year, CancellationToken ct = default)
        {
            return await _db.Patients
                .Where(p => p.PatientNumber != null && p.PatientNumber.EndsWith($"/{year:D2}"))
                .OrderByDescending(p => p.PatientNumber)
                .Select(p => p.PatientNumber)
                .FirstOrDefaultAsync(ct);
        }

        public async Task ReservePatientNumberAsync(string patientNumber, Guid patientId, CancellationToken ct = default)
        {
            var patient = await _db.Patients.FindAsync(new object?[] { patientId }, ct);
            if (patient != null)
            {
                patient.PatientNumber = patientNumber;
                _db.Patients.Update(patient);
                await _db.SaveChangesAsync(ct);
            }
        }

        public async Task<List<Patient>> GetPatientsWithoutNumbersAsync(CancellationToken ct = default)
        {
            return await _db.Patients
                .Where(p => string.IsNullOrEmpty(p.PatientNumber))
                .OrderBy(p => p.CreatedAt)
                .ToListAsync(ct);
        }
        public async Task<long> GetNextSequenceForYearAsync(int year, CancellationToken ct)
        {
            var sequence = await _db.PatientNumberSequences
                .SingleOrDefaultAsync(s => s.Year == year, ct);

            if (sequence == null)
            {
                sequence = new PatientNumberSequence
                {
                    Year = year,
                    LastValue = 1
                };
                _db.PatientNumberSequences.Add(sequence);
            }
            else
            {
                sequence.LastValue++;
            }

            await _db.SaveChangesAsync(ct);
            return sequence.LastValue;
        }


    }
}
