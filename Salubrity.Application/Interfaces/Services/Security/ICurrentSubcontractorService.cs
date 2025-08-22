// Salubrity.Application/Interfaces/Security/ICurrentSubcontractorService.cs
public interface ICurrentSubcontractorService
{
    Task<Guid> GetRequiredSubcontractorIdAsync(Guid userId, CancellationToken ct = default);
}
