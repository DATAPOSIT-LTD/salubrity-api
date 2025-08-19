// Salubrity.Application/Interfaces/Repositories/Identity/IUsersReadRepository.cs
public interface IUsersReadRepository
{
    Task<bool> IsActiveAsync(Guid userId, CancellationToken ct = default);
}