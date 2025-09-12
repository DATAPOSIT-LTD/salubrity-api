using Salubrity.Application.Interfaces.Repositories.Patients;

namespace Salubrity.Application.Services.Patients
{
    public class PatientNumberGeneratorService : IPatientNumberGeneratorService
    {
        private readonly IPatientNumberRepository _repo;

        public PatientNumberGeneratorService(IPatientNumberRepository repo)
        {
            _repo = repo;
        }

        public async Task<string> GenerateAsync(CancellationToken ct = default)
        {
            var year = DateTime.UtcNow.Year % 100;

            // Get last used number for the year
            var lastNumber = await _repo.GetLastPatientNumberForYearAsync(year, ct);

            int nextSeq = 1;
            if (lastNumber != null)
            {
                var parts = lastNumber.Split('/');
                if (int.TryParse(parts[0], out var seq))
                    nextSeq = seq + 1;
            }

            int digits = Math.Max(5, nextSeq.ToString().Length);
            return nextSeq.ToString().PadLeft(digits, '0') + $"/{year:D2}";
        }


        public async Task AssignNumbersToExistingPatientsAsync(CancellationToken ct = default)
        {
            var patientsWithoutNumber = await _repo.GetPatientsWithoutNumbersAsync(ct);

            foreach (var patient in patientsWithoutNumber)
            {
                var patientNumber = await GenerateAsync(ct);
                await _repo.ReservePatientNumberAsync(patientNumber, patient.Id, ct);
            }
        }
    }

}
