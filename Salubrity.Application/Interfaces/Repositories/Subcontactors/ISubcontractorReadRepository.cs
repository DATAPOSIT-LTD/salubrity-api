// Salubrity.Application/Interfaces/Repositories/Subcontractors/ISubcontractorReadRepository.cs
public interface ISubcontractorReadRepository
{
    /// <summary>Returns active subcontractor id for a user or null.</summary>
    Task<Guid?> GetActiveIdByUserIdAsync(Guid userId, CancellationToken ct = default);
}