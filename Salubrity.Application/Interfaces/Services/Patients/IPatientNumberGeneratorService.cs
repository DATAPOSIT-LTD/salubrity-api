public interface IPatientNumberGeneratorService
{
    Task<string> GenerateAsync(CancellationToken ct = default);
    Task AssignNumbersToExistingPatientsAsync(CancellationToken ct = default);
}
