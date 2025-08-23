// Salubrity.Application/Interfaces/Security/ICurrentSubcontractorService.cs
public interface ICurrentSubcontractorService
{
    Task<Guid?> TryGetSubcontractorIdAsync(Guid userId, CancellationToken ct = default);
    Task<Guid> GetSubcontractorIdOrThrowAsync(Guid userId, CancellationToken ct = default);
}
