namespace Salubrity.Application.Interfaces.Repositories.Patients
{
    public interface IPatientNumberRepository
    {
        Task<string?> GetLastPatientNumberForYearAsync(int year, CancellationToken ct = default);
        Task ReservePatientNumberAsync(string patientNumber, Guid patientId, CancellationToken ct = default);
        Task<List<Domain.Entities.Identity.Patient>> GetPatientsWithoutNumbersAsync(CancellationToken ct = default);
    }
}
